import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { ListaPessoasComponent } from './pages/lista-pessoas/lista-pessoas.component';
import { CadastroPessoaComponent } from './pages/cadastro-pessoa/cadastro-pessoa.component';
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
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';

const routes: Routes = [
  {
    path: '',
    component: ListaPessoasComponent
  },
  {
    path: 'novo',
    component: CadastroPessoaComponent
  },
  {
    path: 'editar/:id',
    component: CadastroPessoaComponent
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
    DatePickerModule,
    ListaPessoasComponent
  ],
  providers: [
    FilterService,
    SortService,
    PageService,
    EditService,
    ToolbarService
  ]
})
export class PessoasModule { } 