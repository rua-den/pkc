import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../api.service';
import { WorkPlay } from '../models';

@Component({
  selector: 'app-workplays',
  standalone: true,
  imports: [FormsModule],
  template: `
    <h1>Staff WorkPlay</h1>
    <p>Purchase tasks are automatically created when customer orders cannot be fulfilled from stock.</p>
    <table>
      <thead><tr><th>ID</th><th>Order</th><th>Task</th><th>Need</th><th>Status</th><th>Actions</th></tr></thead>
      <tbody>
        @for (workPlay of workPlays; track workPlay.id) {
          <tr>
            <td>#{{ workPlay.id }}</td><td>#{{ workPlay.orderId }}</td>
            <td><strong>{{ workPlay.type }}</strong><br>{{ workPlay.cardName }}<br><small>{{ workPlay.reason }}</small></td>
            <td>{{ workPlay.quantityToBuy }}</td><td><span class="badge">{{ workPlay.status }}</span></td>
            <td>
              @if (hasPermission('ManageWorkPlay') && workPlay.status === 'Open') {
                <button (click)="startWorkPlay(workPlay.id)">Start</button>
              }
              @if (hasPermission('ManageWorkPlay') && workPlay.status === 'InProgress') {
                <input type="number" min="1" [(ngModel)]="purchased[workPlay.id]">
                <button (click)="completeWorkPlay(workPlay.id)">Complete</button>
              }
            </td>
          </tr>
        }
      </tbody>
    </table>
  `
})
export class WorkPlaysComponent implements OnInit {
  private readonly api = inject(ApiService);
  workPlays: WorkPlay[] = [];
  purchased: Record<number, number> = {};

  ngOnInit() { this.reload(); }
  hasPermission(permission: string) { return permission === 'ManageWorkPlay'; }
  reload() { this.api.getWorkPlays().subscribe(items => { this.workPlays = items; for (const item of items) this.purchased[item.id] ??= item.quantityToBuy; }); }
  startWorkPlay(id: number) { this.api.startWorkPlay(id).subscribe(() => this.reload()); }
  completeWorkPlay(id: number) { this.api.completeWorkPlay(id, this.purchased[id] ?? 1).subscribe(() => this.reload()); }
}
