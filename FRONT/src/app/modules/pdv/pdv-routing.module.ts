import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/guards/auth.guard';
import { PdvShellComponent } from './pages/pdv-shell/pdv-shell.component';
import { SalesListComponent } from './pages/sales-list/sales-list.component';
import { SalesReceiptComponent } from './pages/sales-receipt/sales-receipt.component';

const routes: Routes = [
  { path: '', component: PdvShellComponent, canActivate: [AuthGuard] },
  { path: 'vendas', component: SalesListComponent, canActivate: [AuthGuard] },
  { path: 'vendas/:id/cupom', component: SalesReceiptComponent, canActivate: [AuthGuard] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PdvRoutingModule {}
