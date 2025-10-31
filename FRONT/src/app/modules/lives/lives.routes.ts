import { Routes } from '@angular/router';
import { ListaLivesComponent } from './pages/lista-lives/lista-lives.component';
import { CadastroLiveComponent } from './pages/cadastro-live/cadastro-live.component';
import { ContabilizacaoLiveComponent } from './pages/contabilizacao-live/contabilizacao-live.component';
import { AuthGuard } from '../auth/guards/auth.guard';
import { PermissionGuard } from '../auth/guards/permission.guard';

export const LIVES_ROUTES: Routes = [
  {
    path: '',
    component: ListaLivesComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['lives_manage', 'full_access'] }
  },
  {
    path: 'novo',
    component: CadastroLiveComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['lives_manage', 'full_access'] }
  },
  {
    path: 'editar/:id',
    component: CadastroLiveComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['lives_manage', 'full_access'] }
  },
  {
    path: 'contabilizacao/:id',
    component: ContabilizacaoLiveComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['lives_manage', 'full_access'] }
  }
];
