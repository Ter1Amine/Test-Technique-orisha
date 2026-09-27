import { useState, type FormEvent } from 'react'
import { ApiValidationError, createCommand, type Command } from './api'

interface ProductForm {
  refId: string
  name: string
  color: string
  size: string
  quantity: string
}

interface CartonForm {
  cartonId: string
  products: ProductForm[]
}

interface PaletteForm {
  paletteId: string
  cartons: CartonForm[]
}

interface CommandFormState {
  commandId: string
  palettes: PaletteForm[]
}

const newProduct = (): ProductForm => ({ refId: '', name: '', color: '', size: '', quantity: '1' })
const newCarton = (): CartonForm => ({ cartonId: '', products: [newProduct()] })
const newPalette = (): PaletteForm => ({ paletteId: '', cartons: [newCarton()] })
const newCommand = (): CommandFormState => ({ commandId: '', palettes: [newPalette()] })

function validate(form: CommandFormState): string[] {
  const errors: string[] = []
  const ids = new Set<string>()
  const checkId = (id: string, label: string) => {
    const value = id.trim()
    if (!value) errors.push(`L'identifiant ${label} est obligatoire.`)
    else if (ids.has(value)) errors.push(`L'identifiant ${value} est utilisé plusieurs fois.`)
    else ids.add(value)
  }

  checkId(form.commandId, 'de la commande')
  form.palettes.forEach((palette) => {
    checkId(palette.paletteId, 'de palette')
    palette.cartons.forEach((carton) => {
      checkId(carton.cartonId, 'de carton')
      carton.products.forEach((product) => {
        checkId(product.refId, 'de produit')
        if (!product.name.trim() || !product.color.trim() || !product.size.trim())
          errors.push(`Le produit ${product.refId || '?'} doit avoir un nom, une couleur et une taille.`)
        const quantity = Number(product.quantity)
        if (!Number.isInteger(quantity) || quantity <= 0)
          errors.push(`La quantité du produit ${product.refId || '?'} doit être un entier supérieur à 0.`)
      })
    })
  })
  return errors
}

interface Props {
  onCreated: (command: Command) => void
  onCancel: () => void
}

function CommandForm({ onCreated, onCancel }: Props) {
  const [form, setForm] = useState<CommandFormState>(newCommand)
  const [errors, setErrors] = useState<string[]>([])
  const [saving, setSaving] = useState(false)

  const update = (mutate: (draft: CommandFormState) => void) => {
    setForm((current) => {
      const draft = structuredClone(current)
      mutate(draft)
      return draft
    })
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    const validationErrors = validate(form)
    setErrors(validationErrors)
    if (validationErrors.length > 0) return

    setSaving(true)
    try {
      const created = await createCommand({
        commandId: form.commandId.trim(),
        palettes: form.palettes.map((palette) => ({
          paletteId: palette.paletteId.trim(),
          cartons: palette.cartons.map((carton) => ({
            cartonId: carton.cartonId.trim(),
            products: carton.products.map((product) => ({
              refId: product.refId.trim(),
              name: product.name.trim(),
              color: product.color.trim(),
              size: product.size.trim(),
              quantity: Number(product.quantity),
            })),
          })),
        })),
      })
      onCreated(created)
    } catch (e) {
      setErrors(e instanceof ApiValidationError ? e.errors : [`Erreur lors de l'enregistrement : ${(e as Error).message}`])
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className="command-form" onSubmit={handleSubmit}>
      <h2>Nouvelle commande</h2>

      <label>
        Identifiant de la commande
        <input
          value={form.commandId}
          onChange={(e) => update((d) => { d.commandId = e.target.value })}
        />
      </label>

      {form.palettes.map((palette, pi) => (
        <fieldset key={pi} className="level palette">
          <legend>Palette {pi + 1}</legend>
          <div className="row">
            <input
              placeholder="Identifiant de la palette"
              value={palette.paletteId}
              onChange={(e) => update((d) => { d.palettes[pi].paletteId = e.target.value })}
            />
            {form.palettes.length > 1 && (
              <button type="button" className="danger" onClick={() => update((d) => { d.palettes.splice(pi, 1) })}>
                Supprimer la palette
              </button>
            )}
          </div>

          {palette.cartons.map((carton, ci) => (
            <fieldset key={ci} className="level carton">
              <legend>Carton {ci + 1}</legend>
              <div className="row">
                <input
                  placeholder="Identifiant du carton"
                  value={carton.cartonId}
                  onChange={(e) => update((d) => { d.palettes[pi].cartons[ci].cartonId = e.target.value })}
                />
                {palette.cartons.length > 1 && (
                  <button type="button" className="danger" onClick={() => update((d) => { d.palettes[pi].cartons.splice(ci, 1) })}>
                    Supprimer le carton
                  </button>
                )}
              </div>

              {carton.products.map((product, ri) => {
                const setField = (field: keyof ProductForm, value: string) =>
                  update((d) => { d.palettes[pi].cartons[ci].products[ri][field] = value })
                return (
                  <div key={ri} className="row product-row">
                    <input placeholder="Référence" value={product.refId} onChange={(e) => setField('refId', e.target.value)} />
                    <input placeholder="Nom" value={product.name} onChange={(e) => setField('name', e.target.value)} />
                    <input placeholder="Couleur" value={product.color} onChange={(e) => setField('color', e.target.value)} />
                    <input placeholder="Taille" value={product.size} onChange={(e) => setField('size', e.target.value)} />
                    <input
                      type="number"
                      min={1}
                      placeholder="Quantité"
                      value={product.quantity}
                      onChange={(e) => setField('quantity', e.target.value)}
                    />
                    {carton.products.length > 1 && (
                      <button
                        type="button"
                        className="danger"
                        onClick={() => update((d) => { d.palettes[pi].cartons[ci].products.splice(ri, 1) })}
                      >
                        ✕
                      </button>
                    )}
                  </div>
                )
              })}

              <button type="button" onClick={() => update((d) => { d.palettes[pi].cartons[ci].products.push(newProduct()) })}>
                + Ajouter un produit
              </button>
            </fieldset>
          ))}

          <button type="button" onClick={() => update((d) => { d.palettes[pi].cartons.push(newCarton()) })}>
            + Ajouter un carton
          </button>
        </fieldset>
      ))}

      <button type="button" onClick={() => update((d) => { d.palettes.push(newPalette()) })}>
        + Ajouter une palette
      </button>

      {errors.length > 0 && (
        <ul className="error">
          {errors.map((error, i) => <li key={i}>{error}</li>)}
        </ul>
      )}

      <div className="actions">
        <button type="button" onClick={onCancel} disabled={saving}>Annuler</button>
        <button type="submit" className="primary" disabled={saving}>
          {saving ? 'Enregistrement...' : 'Enregistrer'}
        </button>
      </div>
    </form>
  )
}

export default CommandForm
