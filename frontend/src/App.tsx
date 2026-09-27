import { useEffect, useState } from 'react'
import {
  getAllCommands,
  updateReceipt,
  type Carton,
  type Command,
  type Palette,
  type Product,
  type ReceiptStatus,
  type ReceiptTarget,
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

type UpdateReceipt = (target: ReceiptTarget, isReceived: boolean) => Promise<void>

function ReceiptEditor({ target, status, onUpdate }: {
  target: ReceiptTarget
  status: ReceiptStatus
  onUpdate: UpdateReceipt
}) {
  const [editing, setEditing] = useState(false)
  const [value, setValue] = useState(status === 'Received')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Keeps clicks inside <summary> from toggling the collapsible.
  const stop = (e: React.SyntheticEvent) => {
    e.stopPropagation()
    if (e.type === 'click' && (e.target as HTMLElement).tagName === 'BUTTON') e.preventDefault()
  }

  if (!editing) {
    return (
      <button
        type="button"
        onClick={(e) => {
          stop(e)
          setValue(status === 'Received')
          setError(null)
          setEditing(true)
        }}
      >
        Modifier
      </button>
    )
  }

  const save = async () => {
    setSaving(true)
    setError(null)
    try {
      await onUpdate(target, value)
      setEditing(false)
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setSaving(false)
    }
  }

  return (
    <span className="receipt-editor" onClick={stop} onKeyDown={stop} onKeyUp={stop}>
      <select value={value ? 'true' : 'false'} onChange={(e) => setValue(e.target.value === 'true')} disabled={saving}>
        <option value="true">Reçu</option>
        <option value="false">Non reçu</option>
      </select>
      <button type="button" className="primary" onClick={save} disabled={saving}>
        {saving ? '...' : 'Enregistrer'}
      </button>
      <button type="button" onClick={() => setEditing(false)} disabled={saving}>
        Annuler
      </button>
      {error && <span className="error">{error}</span>}
    </span>
  )
}

function ProductTable({ products, onUpdate }: { products: Product[]; onUpdate: UpdateReceipt }) {
  if (products.length === 0) return <p className="empty">Aucun produit.</p>

  return (
    <table>
      <thead>
        <tr>
          <th>Référence</th>
          <th>Nom</th>
          <th>Couleur</th>
          <th>Taille</th>
          <th>Quantité</th>
          <th>Statut</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {products.map((product) => {
          const status: ReceiptStatus = product.isReceived ? 'Received' : 'NotReceived'
          return (
            <tr key={product.refId}>
              <td>{product.refId}</td>
              <td>{product.name}</td>
              <td>{product.color}</td>
              <td>{product.size}</td>
              <td>{product.quantity}</td>
              <td>
                <StatusBadge status={status} />
              </td>
              <td>
                <ReceiptEditor target={{ level: 'product', id: product.refId }} status={status} onUpdate={onUpdate} />
              </td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}

function CartonItem({ carton, onUpdate }: { carton: Carton; onUpdate: UpdateReceipt }) {
  return (
    <details className="level carton">
      <summary>
        <span className="title">Carton {carton.cartonId}</span>
        <span className="count">{carton.receivedPercent}</span>
        <StatusBadge status={carton.status} />
        <ReceiptEditor target={{ level: 'carton', id: carton.cartonId }} status={carton.status} onUpdate={onUpdate} />
      </summary>
      <ProductTable products={carton.products} onUpdate={onUpdate} />
    </details>
  )
}

function PaletteItem({ palette, onUpdate }: { palette: Palette; onUpdate: UpdateReceipt }) {
  return (
    <details className="level palette">
      <summary>
        <span className="title">Palette {palette.paletteId}</span>
        <span className="count">{palette.receivedPercent}</span>
        <StatusBadge status={palette.status} />
        <ReceiptEditor target={{ level: 'palette', id: palette.paletteId }} status={palette.status} onUpdate={onUpdate} />
      </summary>
      {palette.cartons.length === 0 && <p className="empty">Aucun carton.</p>}
      {palette.cartons.map((carton) => (
        <CartonItem key={carton.cartonId} carton={carton} onUpdate={onUpdate} />
      ))}
    </details>
  )
}

function CommandItem({ command, onChange }: { command: Command; onChange: (command: Command) => void }) {
  const onUpdate: UpdateReceipt = async (target, isReceived) => {
    onChange(await updateReceipt(command.commandId, target, isReceived))
  }

  return (
    <details className="level command">
      <summary>
        <span className="title">Commande {command.commandId}</span>
        <span className="count">{command.receivedPercent}</span>
        <StatusBadge status={command.status} />
        <ReceiptEditor target={{ level: 'command' }} status={command.status} onUpdate={onUpdate} />
      </summary>
      {command.palettes.length === 0 && <p className="empty">Aucune palette.</p>}
      {command.palettes.map((palette) => (
        <PaletteItem key={palette.paletteId} palette={palette} onUpdate={onUpdate} />
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
