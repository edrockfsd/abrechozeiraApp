import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { ListaProdutosComponent } from './pages/lista-produtos/lista-produtos.component';
import { CadastroProdutoComponent } from './pages/cadastro-produto/cadastro-produto.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { 
  GridModule, 
  PagerModule, 
  FilterService, 
  SortService, 
  PageService, 
  EditService, 
  ToolbarService
} from '@syncfusion/ej2-angular-grids';
import { 
  TextBoxModule, 
  NumericTextBoxModule 
} from '@syncfusion/ej2-angular-inputs';
import { 
  DropDownListModule 
} from '@syncfusion/ej2-angular-dropdowns';
import { 
  ButtonModule,
  CheckBoxModule 
} from '@syncfusion/ej2-angular-buttons';

const routes: Routes = [
  {
    path: '',
    component: ListaProdutosComponent
  },
  {
    path: 'novo',
    component: CadastroProdutoComponent
  },
  {
    path: 'editar/:id',
    component: CadastroProdutoComponent
  }
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    SharedModule,
    GridModule,
    PagerModule,
    TextBoxModule,
    NumericTextBoxModule,
    DropDownListModule,
    ButtonModule,
    CheckBoxModule,
    ListaProdutosComponent,
    CadastroProdutoComponent
  ],
  providers: [
    FilterService,
    SortService,
    PageService,
    EditService,
    ToolbarService
  ]
})
export class ProdutosModule { }
