import { Routes } from '@angular/router';
import { AuthGuard } from './modules/auth/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'produtos',
    loadChildren: () => import('./modules/produtos/produtos.module').then(m => m.ProdutosModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'estoque',
    loadChildren: () => import('./modules/estoque/estoque.module').then(m => m.EstoqueModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'pessoas',
    loadChildren: () => import('./modules/pessoas/pessoas.module').then(m => m.PessoasModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'lives',
    loadChildren: () => import('./modules/lives/lives.module').then(m => m.LivesModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'arremates',
    loadChildren: () => import('./modules/arremates/arremates.module').then(m => m.ArrematesModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'pdv',
    loadChildren: () => import('./modules/pdv/pdv.module').then(m => m.PdvModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'caixa',
    loadChildren: () => import('./modules/caixa/caixa.module').then(m => m.CaixaModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'pedidos',
    loadChildren: () => import('./modules/pedidos/pedidos.module').then(m => m.PedidosModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'live-sessions',
    loadChildren: () => import('./modules/live-sessions/live-sessions.module').then(m => m.LiveSessionsModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'envios',
    loadChildren: () => import('./modules/envios/envios.module').then(m => m.EnviosModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'cadastro-cliente',
    loadComponent: () => import('./modules/portal-cliente/cadastro-cliente/cadastro-cliente.component').then(m => m.CadastroClienteComponent)
  },
  {
    path: 'auth',
    loadChildren: () => import('./modules/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: 'user-management',
    loadChildren: () => import('./modules/user-management/user-management.module').then(m => m.UserManagementModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'home',
    loadChildren: () => import('./modules/home/home.module').then(m => m.HomeModule),
    canActivate: [AuthGuard]
  },
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'auth/login'
  }
];
