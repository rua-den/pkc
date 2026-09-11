import { Component, inject, OnInit } from '@angular/core';
import { ApiService } from '../api.service';
import { Delivery } from '../models';

@Component({
  selector: 'app-deliveries',
  standalone: true,
  template: `
    <h1>Deliveries</h1>
    <table>
      <thead><tr><th>Delivery</th><th>Order</th><th>Address</th><th>Status</th><th>Actions</th></tr></thead>
      <tbody>
        @for (delivery of deliveries; track delivery.id) {
          <tr>
            <td>#{{ delivery.id }}</td><td>#{{ delivery.orderId }}</td><td>{{ delivery.address }}</td><td><span class="badge">{{ delivery.status }}</span></td>
            <td>
              @if (hasPermission('ManageDelivery') && delivery.status === 'Pending') {
                <button (click)="dispatchDelivery(delivery.id)">Dispatch</button>
              }
              @if (hasPermission('ManageDelivery') && delivery.status === 'Dispatched') {
                <button (click)="markDelivered(delivery.id)">Mark delivered</button>
              }
            </td>
          </tr>
        }
      </tbody>
    </table>
  `
})
export class DeliveriesComponent implements OnInit {
  private readonly api = inject(ApiService);
  deliveries: Delivery[] = [];
  ngOnInit() { this.reload(); }
  hasPermission(permission: string) { return permission === 'ManageDelivery'; }
  reload() { this.api.getDeliveries().subscribe(items => this.deliveries = items); }
  dispatchDelivery(id: number) { this.api.dispatchDelivery(id).subscribe(() => this.reload()); }
  markDelivered(id: number) { this.api.markDelivered(id).subscribe(() => this.reload()); }
}
