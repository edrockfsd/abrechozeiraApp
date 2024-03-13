import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { VendasComponent } from './vendas.component';
import { ListarComponent } from './listar/listar.component';
import { CadastroComponent } from './cadastro/cadastro.component';
import { UploadComponent } from './upload/upload.component';

const routes: Routes = [
  {  
    path: '', component:VendasComponent,
    children:[{
      path: 'listar',
      component: ListarComponent
    },
    {
      path: 'cadastro',
      component: CadastroComponent
    },
    {
      path: 'upload',
      component: UploadComponent
    }]

  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class VendasRoutingModule{

}
