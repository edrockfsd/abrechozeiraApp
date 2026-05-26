import { Routes } from '@angular/router';
import { ArremateComponent } from './pages/arremate/arremate.component';
import { AuthGuard } from '../auth/guards/auth.guard';

export const ARREMATES_ROUTES: Routes = [
  {
    path: '',
    component: ArremateComponent,
    canActivate: [AuthGuard],
    title: 'Arremate'
  }
]; 
