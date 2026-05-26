import { Routes } from '@angular/router';
import { ListaLiveSessionsComponent } from './pages/lista-live-sessions/lista-live-sessions.component';
import { RelatorioLiveSessionComponent } from './pages/relatorio-live-session/relatorio-live-session.component';
import { AuthGuard } from '../auth/guards/auth.guard';
import { PermissionGuard } from '../auth/guards/permission.guard';

export const LIVSESSIONS_ROUTES: Routes = [
  {
    path: '',
    component: ListaLiveSessionsComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['relatorios_read', 'full_access'] }
  },
  {
    path: ':id/relatorio',
    component: RelatorioLiveSessionComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['relatorios_read', 'full_access'] }
  }
]; 
