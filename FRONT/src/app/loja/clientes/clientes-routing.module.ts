import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ClientesComponent } from './clientes.component';
import { CadastroComponent } from './cadastro/cadastro.component';


const routes: Routes = [
  {  
    path: '', component:ClientesComponent,
    children:[{
      path: 'listar',
      component: ClientesComponent
    },
    {
      path: 'cadastro',
      component: CadastroComponent
    },
    {
      path: 'upload',
      component: ClientesComponent
    }]

  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class ClientesRoutingModule{

}
