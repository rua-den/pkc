import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <nav><div class="shell">
      <strong>⚡ PokeTrade Ops</strong>
      <a routerLink="/catalog" routerLinkActive="active">Catalog</a>
      <a routerLink="/orders" routerLinkActive="active">Orders</a>
      <a routerLink="/workplays" routerLinkActive="active">WorkPlay</a>
      <a routerLink="/deliveries" routerLinkActive="active">Delivery</a>
    </div></nav>
    <main class="shell"><router-outlet /></main>
  `
})
export class AppComponent {}
