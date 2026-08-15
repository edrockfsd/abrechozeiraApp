import { Routes } from '@angular/router';
import { ArremateComponent } from './pages/arremate/arremate.component';
import { ImportarLiveComponent } from './pages/importar-live/importar-live.component';
import { AuthGuard } from '../auth/guards/auth.guard';

export const ARREMATES_ROUTES: Routes = [
  {
    path: '',
    component: ArremateComponent,
    canActivate: [AuthGuard],
    title: 'Arremates'
  },
  {
    path: 'importar',
    component: ImportarLiveComponent,
    canActivate: [AuthGuard],
    title: 'Importar Planilha Live'
  }
];
