import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../auth/guards/auth.guard';
import { PermissionGuard } from '../auth/guards/permission.guard';
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
    component: ListaProdutosComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['produtos_read', 'full_access'] }
  },
  {
    path: 'novo',
    component: CadastroProdutoComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['produtos_create', 'produtos_update', 'full_access'] }
  },
  {
    path: 'editar/:id',
    component: CadastroProdutoComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['produtos_update', 'full_access'] }
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
