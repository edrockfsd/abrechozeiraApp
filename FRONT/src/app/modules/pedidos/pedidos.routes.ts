import { Routes } from '@angular/router';
import { ListaPedidosComponent } from './pages/lista-pedidos/lista-pedidos.component';
import { CadastroPedidoComponent } from './pages/cadastro-pedido/cadastro-pedido.component';
import { AuthGuard } from '../auth/guards/auth.guard';
import { PermissionGuard } from '../auth/guards/permission.guard';

export const PEDIDOS_ROUTES: Routes = [
  {
    path: '',
    component: ListaPedidosComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['pedidos_read', 'full_access'] }
  },
  {
    path: 'novo',
    component: CadastroPedidoComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['pedidos_create', 'full_access'] }
  },
  {
    path: 'novo/:pessoaId',
    component: CadastroPedidoComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['pedidos_create', 'full_access'] }
  },
  {
    path: 'editar/:id',
    component: CadastroPedidoComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['pedidos_create', 'pedidos_read', 'full_access'] }
  }
]; 
