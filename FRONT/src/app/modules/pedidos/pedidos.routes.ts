import { Routes } from '@angular/router';
import { ListaPedidosComponent } from './pages/lista-pedidos/lista-pedidos.component';
import { CadastroPedidoComponent } from './pages/cadastro-pedido/cadastro-pedido.component';

export const PEDIDOS_ROUTES: Routes = [
  {
    path: '',
    component: ListaPedidosComponent
  },
  {
    path: 'novo',
    component: CadastroPedidoComponent
  },
  {
    path: 'novo/:pessoaId',
    component: CadastroPedidoComponent
  },
  {
    path: 'editar/:pessoaId/:id',
    component: CadastroPedidoComponent
  }
]; 