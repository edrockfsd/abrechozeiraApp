import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from './modules/auth/services/auth.service';

interface MenuItem {
  label: string;
  route: string;
  icon?: string; // material icon name
  permissions?: string[];
  roles?: string[];
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class AppComponent {
  constructor(private auth: AuthService, private router: Router) {
    this.initTheme();
    this.initSidebar();
  }

  menu: MenuItem[] = [
    { label: 'Home', route: '/home', icon: 'dashboard' },
    { label: 'PDV', route: '/pdv', icon: 'point_of_sale' },
    { label: 'Caixa', route: '/caixa', icon: 'account_balance_wallet' },
    { label: 'Vendas PDV', route: '/pdv/vendas', icon: 'list_alt' },
    { label: 'Produtos', route: '/produtos', icon: 'shopping_bag', permissions: ['produtos_read', 'full_access'] },
    { label: 'Pedidos', route: '/pedidos', icon: 'receipt_long', permissions: ['pedidos_read', 'full_access'] },
    { label: 'Pessoas', route: '/pessoas', icon: 'group', permissions: ['clientes_read', 'full_access'] },
    { label: 'Estoque', route: '/estoque', icon: 'inventory_2', permissions: ['estoque_read', 'full_access'] },
    { label: 'Lives', route: '/lives', icon: 'smart_display', permissions: ['lives_manage', 'full_access'] },
    { label: 'Relatórios', route: '/live-sessions', icon: 'bar_chart', permissions: ['relatorios_read', 'full_access'] },
    { label: 'Usuários', route: '/user-management', icon: 'admin_panel_settings', roles: ['ADMIN'] },
    { label: 'Arremates', route: '/arremates', icon: 'gavel' }
  ];

  get isAuthenticated(): boolean {
    return this.auth.isAuthenticated();
  }

  get userName(): string {
    return this.auth.getCurrentUser()?.name || '';
  }

  get isAuthRoute(): boolean {
    const url = this.router.url || '';
    return url.startsWith('/auth');
  }

  pageTitle = 'Painel';

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

  onLogout(): void {
    this.auth.logout();
    this.router.navigate(['/auth/login']);
  }

  isDark = false;
  private initTheme(): void {
    const saved = localStorage.getItem('theme');
    this.isDark = saved === 'dark';
    this.applyTheme();
  }

  toggleTheme(): void {
    this.isDark = !this.isDark;
    localStorage.setItem('theme', this.isDark ? 'dark' : 'light');
    this.applyTheme();
  }

  private applyTheme(): void {
    const root = document.documentElement;
    if (this.isDark) root.classList.add('dark');
    else root.classList.remove('dark');
  }

  // Sidebar collapse state
  isCollapsed = false;
  private initSidebar(): void {
    const saved = localStorage.getItem('sidebar-collapsed');
    this.isCollapsed = saved === 'true';
  }

  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
    localStorage.setItem('sidebar-collapsed', String(this.isCollapsed));
  }
}
