import { Routes } from '@angular/router';
import { ListaProdutosComponent } from './modules/produtos/pages/lista-produtos/lista-produtos.component';
import { CadastroProdutoComponent } from './modules/produtos/pages/cadastro-produto/cadastro-produto.component';
import { ListaEstoqueComponent } from './modules/estoque/pages/lista-estoque/lista-estoque.component';

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
    path: '',
    redirectTo: 'produtos',
    pathMatch: 'full'
  }
]; 