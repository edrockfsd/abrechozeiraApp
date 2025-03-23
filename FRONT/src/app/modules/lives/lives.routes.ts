import { Routes } from '@angular/router';
import { ListaLivesComponent } from './pages/lista-lives/lista-lives.component';
import { CadastroLiveComponent } from './pages/cadastro-live/cadastro-live.component';

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
  }
]; 