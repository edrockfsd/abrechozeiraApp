import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';
import { KpiCardComponent } from '../../../../shared/components/kpi-card/kpi-card.component';

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
  imports: [CommonModule, RouterModule, KpiCardComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  constructor(private auth: AuthService) {}

  menu: MenuItem[] = [
    { label: 'Produtos', route: '/produtos', icon: 'shopping_bag', permissions: ['produtos_read', 'full_access'] },
    { label: 'Pedidos', route: '/pedidos', icon: 'receipt_long', permissions: ['pedidos_read', 'full_access'] },
    { label: 'Pessoas', route: '/pessoas', icon: 'group', permissions: ['clientes_read', 'full_access'] },
    { label: 'Estoque', route: '/estoque', icon: 'inventory_2', permissions: ['estoque_read', 'full_access'] },
    { label: 'Lives', route: '/lives', icon: 'smart_display', permissions: ['lives_manage', 'full_access'] },
    { label: 'Relatórios', route: '/live-sessions', icon: 'bar_chart', permissions: ['relatorios_read', 'full_access'] },
    { label: 'Usuários', route: '/user-management', icon: 'admin_panel_settings', roles: ['ADMIN'] },
    { label: 'Arremates', route: '/arremates', icon: 'gavel', permissions: ['full_access'] }
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
