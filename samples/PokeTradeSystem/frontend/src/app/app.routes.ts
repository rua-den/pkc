import { Routes } from '@angular/router';
import { CatalogComponent } from './pages/catalog.component';
import { OrdersComponent } from './pages/orders.component';
import { WorkPlaysComponent } from './pages/workplays.component';
import { DeliveriesComponent } from './pages/deliveries.component';

export const routes: Routes = [
  { path: '', redirectTo: 'catalog', pathMatch: 'full' },
  { path: 'catalog', component: CatalogComponent },
  { path: 'orders', component: OrdersComponent },
  { path: 'workplays', component: WorkPlaysComponent },
  { path: 'deliveries', component: DeliveriesComponent }
];
