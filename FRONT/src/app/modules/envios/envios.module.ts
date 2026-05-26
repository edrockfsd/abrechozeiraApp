import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { enviosRoutes } from './envios.routes';

@NgModule({
  imports: [RouterModule.forChild(enviosRoutes)]
})
export class EnviosModule {}
