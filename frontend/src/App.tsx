import { useEffect, useMemo, useRef, useState } from 'react'
import {
  getAllCommands,
  getCommandById,
  updateProductReceipt,
  type Carton,
  type Command,
  type Palette,
  type Product,
  type ReceiptStatus,
} from './api'
import CommandForm from './CommandForm'
import './App.css'

const statusLabels: Record<ReceiptStatus, string> = {
  Received: 'Reçu',
  NotReceived: 'Non reçu',
  PartiallyReceived: 'Partiellement reçu',
}

function StatusBadge({ status }: { status: ReceiptStatus }) {
  return <span className={`badge ${status}`}>{statusLabels[status] ?? status}</span>
}

type Draft = Record<string, boolean>
type ToggleProducts = (refIds: string[], isReceived: boolean) => void

function computeStatus(received: number, total: number): ReceiptStatus {
  if (total === 0 || received === 0) return 'NotReceived'
  return received === total ? 'Received' : 'PartiallyReceived'
}

function applyDraft(command: Command, draft: Draft): Command {
  let commandReceived = 0
  let commandTotal = 0
  const palettes = command.palettes.map((palette) => {
    let paletteReceived = 0
    let paletteTotal = 0
    const cartons = palette.cartons.map((carton) => {
      const products = carton.products.map((product) =>
        product.refId in draft ? { ...product, isReceived: draft[product.refId] } : product,
      )
      const received = products.filter((p) => p.isReceived).length
      paletteReceived += received
      paletteTotal += products.length
      return {
        ...carton,
        products,
        status: computeStatus(received, products.length),
        receivedPercent: `${received}/${products.length}`,
      }
    })
    commandReceived += paletteReceived
    commandTotal += paletteTotal
    return {
      ...palette,
      cartons,
      status: computeStatus(paletteReceived, paletteTotal),
      receivedPercent: `${paletteReceived}/${paletteTotal}`,
    }
  })
  return {
    ...command,
    palettes,
    status: computeStatus(commandReceived, commandTotal),
    receivedPercent: `${commandReceived}/${commandTotal}`,
  }
}

const productsOf = (cartons: Carton[]) => cartons.flatMap((carton) => carton.products)

function GroupCheckbox({ products, onToggle, disabled }: {
  products: Product[]
  onToggle: ToggleProducts
  disabled: boolean
}) {
  const ref = useRef<HTMLInputElement>(null)
  const received = products.filter((p) => p.isReceived).length
  const checked = products.length > 0 && received === products.length

  useEffect(() => {
    if (ref.current) ref.current.indeterminate = received > 0 && !checked
  }, [received, checked])

  return (
    <input
      ref={ref}
      type="checkbox"
      title="Tout cocher / décocher"
      checked={checked}
      disabled={disabled || products.length === 0}
      onClick={(e) => e.stopPropagation()}
      onChange={() => onToggle(products.map((p) => p.refId), !checked)}
    />
  )
}

function ProductTable({ products, draft, onToggle, disabled }: {
  products: Product[]
  draft: Draft
  onToggle: ToggleProducts
  disabled: boolean
}) {
  if (products.length === 0) return <p className="empty">Aucun produit.</p>

  return (
    <table>
      <thead>
        <tr>
          <th>Reçu</th>
          <th>Référence</th>
          <th>Nom</th>
          <th>Couleur</th>
          <th>Taille</th>
          <th>Quantité</th>
          <th>Statut</th>
        </tr>
      </thead>
      <tbody>
        {products.map((product) => (
          <tr key={product.refId} className={product.refId in draft ? 'modified' : undefined}>
            <td>
              <input
                type="checkbox"
                checked={product.isReceived}
                disabled={disabled}
                onChange={(e) => onToggle([product.refId], e.target.checked)}
              />
            </td>
            <td>{product.refId}</td>
            <td>{product.name}</td>
            <td>{product.color}</td>
            <td>{product.size}</td>
            <td>{product.quantity}</td>
            <td>
              <StatusBadge status={product.isReceived ? 'Received' : 'NotReceived'} />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

function CartonItem({ carton, draft, onToggle, disabled }: {
  carton: Carton
  draft: Draft
  onToggle: ToggleProducts
  disabled: boolean
}) {
  return (
    <details className="level carton">
      <summary>
        <GroupCheckbox products={carton.products} onToggle={onToggle} disabled={disabled} />
        <span className="title">Carton {carton.cartonId}</span>
        <span className="count">{carton.receivedPercent}</span>
        <StatusBadge status={carton.status} />
      </summary>
      <ProductTable products={carton.products} draft={draft} onToggle={onToggle} disabled={disabled} />
    </details>
  )
}

function PaletteItem({ palette, draft, onToggle, disabled }: {
  palette: Palette
  draft: Draft
  onToggle: ToggleProducts
  disabled: boolean
}) {
  return (
    <details className="level palette">
      <summary>
        <GroupCheckbox products={productsOf(palette.cartons)} onToggle={onToggle} disabled={disabled} />
        <span className="title">Palette {palette.paletteId}</span>
        <span className="count">{palette.receivedPercent}</span>
        <StatusBadge status={palette.status} />
      </summary>
      {palette.cartons.length === 0 && <p className="empty">Aucun carton.</p>}
      {palette.cartons.map((carton) => (
        <CartonItem key={carton.cartonId} carton={carton} draft={draft} onToggle={onToggle} disabled={disabled} />
      ))}
    </details>
  )
}

function CommandItem({ command, onChange }: { command: Command; onChange: (command: Command) => void }) {
  const [draft, setDraft] = useState<Draft>({})
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const savedStates = useMemo(
    () => new Map(command.palettes.flatMap((p) => productsOf(p.cartons)).map((p) => [p.refId, p.isReceived])),
    [command],
  )
  const view = useMemo(() => applyDraft(command, draft), [command, draft])
  const changes = Object.entries(draft)

  const onToggle: ToggleProducts = (refIds, isReceived) => {
    setError(null)
    setDraft((current) => {
      const next = { ...current }
      for (const refId of refIds) {
        if (savedStates.get(refId) === isReceived) delete next[refId]
        else next[refId] = isReceived
      }
      return next
    })
  }

  const save = async (e: React.MouseEvent) => {
    e.preventDefault()
    setSaving(true)
    setError(null)
    let latest: Command | null = null
    try {
      for (const [refId, isReceived] of changes) {
        latest = await updateProductReceipt(command.commandId, refId, isReceived)
      }
      if (latest) onChange(latest)
      setDraft({})
    } catch (err) {
      setError(`Erreur lors de l'enregistrement : ${(err as Error).message}`)
      try {
        const fresh = await getCommandById(command.commandId)
        onChange(fresh)
        setDraft({})
      } catch {
      }
    } finally {
      setSaving(false)
    }
  }

  const cancel = (e: React.MouseEvent) => {
    e.preventDefault()
    setDraft({})
    setError(null)
  }

  return (
    <details className={`level command${changes.length > 0 ? ' dirty' : ''}`}>
      <summary>
        <GroupCheckbox products={view.palettes.flatMap((p) => productsOf(p.cartons))} onToggle={onToggle} disabled={saving} />
        <span className="title">Commande {view.commandId}</span>
        <span className="count">{view.receivedPercent}</span>
        <StatusBadge status={view.status} />
        {changes.length > 0 && (
          <span className="receipt-editor">
            <button type="button" className="primary" onClick={save} disabled={saving}>
              {saving ? 'Enregistrement...' : `Enregistrer (${changes.length})`}
            </button>
            <button type="button" onClick={cancel} disabled={saving}>
              Annuler
            </button>
          </span>
        )}
      </summary>
      {error && <p className="error">{error}</p>}
      {view.palettes.length === 0 && <p className="empty">Aucune palette.</p>}
      {view.palettes.map((palette) => (
        <PaletteItem key={palette.paletteId} palette={palette} draft={draft} onToggle={onToggle} disabled={saving} />
      ))}
    </details>
  )
}

function App() {
  const [commands, setCommands] = useState<Command[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showForm, setShowForm] = useState(false)

  useEffect(() => {
    getAllCommands()
      .then(setCommands)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  return (
    <main className="container">
      <div className="header">
        <h1>Commandes</h1>
        {!showForm && (
          <button type="button" className="primary" onClick={() => setShowForm(true)}>
            + Ajouter une commande
          </button>
        )}
      </div>

      {showForm && (
        <CommandForm
          onCancel={() => setShowForm(false)}
          onCreated={(command) => {
            setCommands((current) => [...current, command])
            setShowForm(false)
          }}
        />
      )}

      {loading && <p>Chargement...</p>}
      {error && <p className="error">Erreur lors du chargement des commandes : {error}</p>}
      {!loading && !error && commands.length === 0 && <p>Aucune commande trouvée.</p>}

      {commands.map((command) => (
        <CommandItem
          key={command.commandId}
          command={command}
          onChange={(updated) =>
            setCommands((current) => current.map((c) => (c.commandId === updated.commandId ? updated : c)))
          }
        />
      ))}
    </main>
  )
}

export default App
