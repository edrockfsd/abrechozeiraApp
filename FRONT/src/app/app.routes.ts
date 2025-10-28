import { Routes } from '@angular/router';
import { ListaProdutosComponent } from './modules/produtos/pages/lista-produtos/lista-produtos.component';
import { CadastroProdutoComponent } from './modules/produtos/pages/cadastro-produto/cadastro-produto.component';
import { ListaEstoqueComponent } from './modules/estoque/pages/lista-estoque/lista-estoque.component';
import { TestDataComponent } from './test-data.component';
import { DebugUsersComponent } from './debug-users.component';
import { ConnectionTestComponent } from './connection-test.component';
import { SimpleTestComponent } from './simple-test.component';
import { DiagnosticComponent } from './diagnostic.component';

export const routes: Routes = [
  {
    path: 'produtos',
    loadChildren: () => import('./modules/produtos/produtos.module').then(m => m.ProdutosModule)
  },
  {
    path: 'estoque',
    loadChildren: () => import('./modules/estoque/estoque.module').then(m => m.EstoqueModule)
  },
  {
    path: 'pessoas',
    loadChildren: () => import('./modules/pessoas/pessoas.module').then(m => m.PessoasModule)
  },
  {
    path: 'lives',
    loadChildren: () => import('./modules/lives/lives.module').then(m => m.LivesModule)
  },
  {
    path: 'arremates',
    loadChildren: () => import('./modules/arremates/arremates.module').then(m => m.ArrematesModule)
  },
  {
    path: 'pedidos',
    loadChildren: () => import('./modules/pedidos/pedidos.module').then(m => m.PedidosModule)
  },
  {
    path: 'live-sessions',
    loadChildren: () => import('./modules/live-sessions/live-sessions.module').then(m => m.LiveSessionsModule)
  },
  {
    path: 'auth',
    loadChildren: () => import('./modules/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: 'user-management',
    loadChildren: () => import('./modules/user-management/user-management.module').then(m => m.UserManagementModule)
  },
  {
    path: 'test-data',
    component: TestDataComponent
  },
  {
    path: 'debug-users',
    component: DebugUsersComponent
  },
  {
    path: 'connection-test',
    component: ConnectionTestComponent
  },
  {
    path: 'simple-test',
    component: SimpleTestComponent
  },
  {
    path: 'diagnostic',
    component: DiagnosticComponent
  },
  {
    path: '',
    redirectTo: 'produtos',
    pathMatch: 'full'
  }
];