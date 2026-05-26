import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';
import { KpiCardComponent } from '../../../../shared/components/kpi-card/kpi-card.component';
import { DashboardService, DashboardData } from '../../../../services/dashboard.service';

interface MenuItem {
  label: string;
  route: string;
  icon?: string;
  permissions?: string[];
  roles?: string[];
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, KpiCardComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  dashboardData: DashboardData | null = null;
  loading = true;
  error: string | null = null;

  constructor(
    private auth: AuthService,
    private dashboardService: DashboardService
  ) { }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.error = null;
    this.dashboardService.getDashboard().subscribe({
      next: (data) => {
        this.dashboardData = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar dashboard:', err);
        this.error = 'Erro ao carregar dados do dashboard';
        this.loading = false;
      }
    });
  }

  menu: MenuItem[] = [
    { label: 'PDV', route: '/pdv', icon: 'point_of_sale', permissions: ['full_access'] },
    { label: 'Produtos', route: '/produtos', icon: 'shopping_bag', permissions: ['produtos_read', 'full_access'] },
    { label: 'Pedidos', route: '/pedidos', icon: 'receipt_long', permissions: ['pedidos_read', 'full_access'] },
    { label: 'Pessoas', route: '/pessoas', icon: 'group', permissions: ['clientes_read', 'full_access'] },
    { label: 'Estoque', route: '/estoque', icon: 'inventory_2', permissions: ['estoque_read', 'full_access'] },
    { label: 'Lives', route: '/lives', icon: 'smart_display', permissions: ['lives_manage', 'full_access'] },
    { label: 'Arremates', route: '/arremates', icon: 'gavel', permissions: ['full_access'] },
    { label: 'Usuários', route: '/user-management', icon: 'admin_panel_settings', roles: ['ADMIN'] }
  ];

  get userName(): string {
    return this.auth.getCurrentUser()?.name || '';
  }

  get vendasHoje(): string {
    if (!this.dashboardData) return '—';
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(this.dashboardData.vendasHoje);
  }

  get vendasSemana(): string {
    if (!this.dashboardData) return '—';
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(this.dashboardData.vendasSemana);
  }

  get vendasMes(): string {
    if (!this.dashboardData) return '—';
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(this.dashboardData.vendasMes);
  }

  get estoqueBaixoCount(): number {
    return this.dashboardData?.estoqueBaixo?.length || 0;
  }

  get proximaLiveInfo(): string {
    if (!this.dashboardData?.proximaLive) return 'Nenhuma';
    const data = new Date(this.dashboardData.proximaLive.data);
    return `${data.toLocaleDateString('pt-BR')}`;
  }

  // Calcula altura da barra do gráfico (max 100%)
  getBarHeight(total: number): number {
    if (!this.dashboardData?.vendasUltimos7Dias) return 0;
    const max = Math.max(...this.dashboardData.vendasUltimos7Dias.map(v => v.total));
    if (max === 0) return 0;
    return (total / max) * 100;
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
