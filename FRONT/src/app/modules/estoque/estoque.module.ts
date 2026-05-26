import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/guards/auth.guard';
import { PermissionGuard } from '../auth/guards/permission.guard';
import { ListaEstoqueComponent } from './pages/lista-estoque/lista-estoque.component';

const routes: Routes = [
  {
    path: '',
    component: ListaEstoqueComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['estoque_read', 'full_access'] }
  }
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ListaEstoqueComponent
  ]
})
export class EstoqueModule { } 
