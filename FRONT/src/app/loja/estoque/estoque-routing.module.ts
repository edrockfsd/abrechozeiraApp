import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { EstoqueComponent } from './estoque.component';
import { CadastroComponent } from './cadastro/cadastro.component';


const routes: Routes = [
  {  
    path: '', component:EstoqueComponent,
    children:[{
      path: 'listar',
      component: EstoqueComponent
    },
    {
      path: 'cadastro',
      component: CadastroComponent
    }]

  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class EstoqueRoutingModule{

}
