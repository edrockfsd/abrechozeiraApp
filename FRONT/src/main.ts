import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';
import { configureSyncfusion } from './app/syncfusion.config';
import 'zone.js';

// Configurando o Syncfusion antes de inicializar a aplicação
configureSyncfusion();

// Inicializando a aplicação Angular
bootstrapApplication(AppComponent, appConfig)
  .catch(err => console.error(err));