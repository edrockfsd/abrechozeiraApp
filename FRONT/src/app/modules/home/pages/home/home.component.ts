import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';

interface MenuItem {
  label: string;
  route: string;
  icon?: string; // optional icon class
  permissions?: string[]; // any of these grants visibility
  roles?: string[]; // any role grants visibility
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  constructor(private auth: AuthService) {}

  menu: MenuItem[] = [
    { label: 'Produtos', route: '/produtos', icon: 'e-icons e-product', permissions: ['produtos_read', 'full_access'] },
    { label: 'Pedidos', route: '/pedidos', icon: 'e-icons e-file', permissions: ['pedidos_read', 'full_access'] },
    { label: 'Pessoas', route: '/pessoas', icon: 'e-icons e-people', permissions: ['clientes_read', 'full_access'] },
    { label: 'Estoque', route: '/estoque', icon: 'e-icons e-database', permissions: ['estoque_read', 'full_access'] },
    { label: 'Lives', route: '/lives', icon: 'e-icons e-video', permissions: ['lives_manage', 'full_access'] },
    { label: 'Relatórios', route: '/live-sessions', icon: 'e-icons e-chart', permissions: ['relatorios_read', 'full_access'] },
    { label: 'Usuários', route: '/user-management', icon: 'e-icons e-people', roles: ['ADMIN'] },
    { label: 'Arremates', route: '/arremates', icon: 'e-icons e-settings', permissions: ['full_access'] }
  ];

  get userName(): string {
    return this.auth.getCurrentUser()?.name || '';
  }

  isVisible(item: MenuItem): boolean {
    if (item.roles && item.roles.length > 0) {
      const ok = item.roles.some(r => this.auth.hasRole(r));
      if (ok) return true;
    }
    if (item.permissions && item.permissions.length > 0) {
      return item.permissions.some(p => this.auth.hasPermission(p));
    }
    return true;
  }
}

