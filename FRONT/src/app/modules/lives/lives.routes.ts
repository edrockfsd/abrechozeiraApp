import { Routes } from '@angular/router';
import { ListaLivesComponent } from './pages/lista-lives/lista-lives.component';
import { CadastroLiveComponent } from './pages/cadastro-live/cadastro-live.component';
import { ContabilizacaoLiveComponent } from './pages/contabilizacao-live/contabilizacao-live.component';

export const LIVES_ROUTES: Routes = [
  {
    path: '',
    component: ListaLivesComponent
  },
  {
    path: 'novo',
    component: CadastroLiveComponent
  },
  {
    path: 'editar/:id',
    component: CadastroLiveComponent
  },
  {
    path: 'contabilizacao/:id',
    component: ContabilizacaoLiveComponent
  }
];