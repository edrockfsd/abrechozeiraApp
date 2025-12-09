import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/guards/auth.guard';
import { CaixaPainelComponent } from './pages/caixa-painel/caixa-painel.component';

const routes: Routes = [
  { path: '', component: CaixaPainelComponent, canActivate: [AuthGuard] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CaixaRoutingModule {}

