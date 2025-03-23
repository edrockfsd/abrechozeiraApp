import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ListaEstoqueComponent } from './pages/lista-estoque/lista-estoque.component';

const routes: Routes = [
  {
    path: '',
    component: ListaEstoqueComponent
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