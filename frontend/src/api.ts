export type ReceiptStatus = 'Received' | 'NotReceived' | 'PartiallyReceived'

export interface Product {
  refId: string
  name: string
  color: string
  size: string
  quantity: number
  isReceived: boolean
}

export interface Carton {
  cartonId: string
  status: ReceiptStatus
  receivedPercent: string
  products: Product[]
}

export interface Palette {
  paletteId: string
  status: ReceiptStatus
  receivedPercent: string
  cartons: Carton[]
}

export interface Command {
  commandId: string
  status: ReceiptStatus
  receivedPercent: string
  palettes: Palette[]
}

const API_URL = import.meta.env.VITE_API_URL

export async function getAllCommands(): Promise<Command[]> {
  const response = await fetch(`${API_URL}/api/commands`)
  if (!response.ok) throw new Error(`HTTP ${response.status}`)
  return response.json()
}

export type ReceiptTarget =
  | { level: 'command' }
  | { level: 'palette' | 'carton' | 'product'; id: string }

const receiptSegments = { palette: 'palettes', carton: 'cartons', product: 'products' } as const

export async function updateReceipt(
  commandId: string,
  target: ReceiptTarget,
  isReceived: boolean,
): Promise<Command> {
  const base = `${API_URL}/api/${encodeURIComponent(commandId)}`
  const path =
    target.level === 'command'
      ? `${base}/receipt`
      : `${base}/${receiptSegments[target.level]}/${encodeURIComponent(target.id)}/receipt`
  const response = await fetch(`${path}?isReceived=${isReceived}`, { method: 'PUT' })
  if (!response.ok) throw new Error(`HTTP ${response.status}`)
  return response.json()
}

export interface CreateCommandRequest {
  commandId: string
  palettes: {
    paletteId: string
    cartons: {
      cartonId: string
      products: {
        refId: string
        name: string
        color: string
        size: string
        quantity: number
      }[]
    }[]
  }[]
}

export class ApiValidationError extends Error {
  errors: string[]

  constructor(errors: string[]) {
    super(errors.join(' '))
    this.errors = errors
  }
}

export async function createCommand(request: CreateCommandRequest): Promise<Command> {
  const response = await fetch(`${API_URL}/api/commands`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })
  if (response.status === 400) {
    const body = await response.json().catch(() => null)
    throw new ApiValidationError(body?.errors ?? ['Requête invalide.'])
  }
  if (!response.ok) throw new Error(`HTTP ${response.status}`)
  return response.json()
}
