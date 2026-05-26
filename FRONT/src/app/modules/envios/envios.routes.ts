import { Routes } from '@angular/router';
import { ListaEnviosComponent } from './pages/lista-envios/lista-envios.component';
import { ImportarEnviosComponent } from './pages/importar-envios/importar-envios.component';

export const enviosRoutes: Routes = [
  {
    path: '',
    component: ListaEnviosComponent
  },
  {
    path: 'importar',
    component: ImportarEnviosComponent
  }
];
