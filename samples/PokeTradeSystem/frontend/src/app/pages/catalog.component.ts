import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { ApiService } from '../api.service';
import { PokemonCard } from '../models';

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [FormsModule, CurrencyPipe],
  template: `
    <h1>Pokémon Card Catalog</h1>
    <div class="card">
      <h3>Create order</h3>
      <input [(ngModel)]="customerName" placeholder="Customer name">
      <input [(ngModel)]="deliveryAddress" placeholder="Delivery address">
      <select [(ngModel)]="selectedCardId">
        @for (card of cards; track card.id) { <option [ngValue]="card.id">{{ card.name }}</option> }
      </select>
      <input type="number" min="1" [(ngModel)]="quantity">
      <button (click)="createOrder()">Place order</button>
      @if (message) { <p class="success">{{ message }}</p> }
      @if (error) { <p class="error">{{ error }}</p> }
    </div>
    <div class="grid">
      @for (card of cards; track card.id) {
        <article class="card">
          <span class="badge">{{ card.rarity }}</span>
          <h3>{{ card.name }}</h3>
          <p>{{ card.setName }}</p>
          <p><strong>{{ card.price | currency }}</strong></p>
          <p>Stock: {{ card.stock }} · Reorder level: {{ card.reorderLevel }}</p>
        </article>
      }
    </div>
  `
})
export class CatalogComponent implements OnInit {
  private readonly api = inject(ApiService);
  cards: PokemonCard[] = [];
  customerName = 'Ash Ketchum';
  deliveryAddress = '25 Pallet Town Road';
  selectedCardId = 1;
  quantity = 1;
  message = '';
  error = '';

  ngOnInit() { this.reload(); }
  reload() { this.api.getCards().subscribe(cards => this.cards = cards); }
  createOrder() {
    this.message = ''; this.error = '';
    this.api.createOrder({ customerName: this.customerName, deliveryAddress: this.deliveryAddress, lines: [{ cardId: this.selectedCardId, quantity: this.quantity }] })
      .subscribe({ next: order => { this.message = `Order #${order.id} created as ${order.status}.`; this.reload(); }, error: response => this.error = response.error?.message ?? 'Order failed.' });
  }
}
