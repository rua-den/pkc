export interface PokemonCard { id: number; name: string; setName: string; rarity: string; price: number; stock: number; reorderLevel: number; }
export interface OrderLine { cardId: number; cardName: string; quantity: number; unitPrice: number; }
export interface Order { id: number; customerName: string; deliveryAddress: string; lines: OrderLine[]; status: string; total: number; }
export interface WorkPlay { id: number; orderId: number; cardId: number; cardName: string; quantityToBuy: number; type: string; reason: string; status: string; purchasedQuantity: number; }
export interface Delivery { id: number; orderId: number; address: string; status: string; dispatchedAt?: string; deliveredAt?: string; }
