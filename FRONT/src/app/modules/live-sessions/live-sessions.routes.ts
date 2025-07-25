import { Routes } from '@angular/router';
import { ListaLiveSessionsComponent } from './pages/lista-live-sessions/lista-live-sessions.component';
import { RelatorioLiveSessionComponent } from './pages/relatorio-live-session/relatorio-live-session.component';

export const LIVSESSIONS_ROUTES: Routes = [
  {
    path: '',
    component: ListaLiveSessionsComponent
  },
  {
    path: ':id/relatorio',
    component: RelatorioLiveSessionComponent
  }
]; 