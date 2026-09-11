import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Delivery, Order, PokemonCard, WorkPlay } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  getCards() { return this.http.get<PokemonCard[]>('/api/cards'); }
  getOrders() { return this.http.get<Order[]>('/api/orders'); }
  createOrder(request: { customerName: string; deliveryAddress: string; lines: { cardId: number; quantity: number }[] }) {
    return this.http.post<Order>('/api/orders', request);
  }
  getWorkPlays() { return this.http.get<WorkPlay[]>('/api/workplays'); }
  startWorkPlay(id: number) { return this.http.patch<WorkPlay>(`/api/workplays/${id}/start`, {}); }
  completeWorkPlay(id: number, purchasedQuantity: number) {
    return this.http.patch<WorkPlay>(`/api/workplays/${id}/complete`, { purchasedQuantity });
  }
  getDeliveries() { return this.http.get<Delivery[]>('/api/deliveries'); }
  dispatchDelivery(id: number) { return this.http.patch<Delivery>(`/api/deliveries/${id}/dispatch`, {}); }
  markDelivered(id: number) { return this.http.patch<Delivery>(`/api/deliveries/${id}/delivered`, {}); }
}
