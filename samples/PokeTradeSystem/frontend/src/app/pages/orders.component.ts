import { Component, inject, OnInit } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { ApiService } from '../api.service';
import { Order } from '../models';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CurrencyPipe],
  template: `
    <h1>Orders</h1>
    <button class="secondary" (click)="reload()">Refresh</button>
    <table>
      <thead><tr><th>Order</th><th>Customer</th><th>Cards</th><th>Status</th><th>Total</th></tr></thead>
      <tbody>
        @for (order of orders; track order.id) {
          <tr>
            <td>#{{ order.id }}</td><td>{{ order.customerName }}</td>
            <td>@for (line of order.lines; track line.cardId) { <div>{{ line.quantity }} × {{ line.cardName }}</div> }</td>
            <td><span class="badge">{{ order.status }}</span></td><td>{{ order.total | currency }}</td>
          </tr>
        }
      </tbody>
    </table>
  `
})
export class OrdersComponent implements OnInit {
  private readonly api = inject(ApiService);
  orders: Order[] = [];
  ngOnInit() { this.reload(); }
  reload() { this.api.getOrders().subscribe(orders => this.orders = orders); }
}
