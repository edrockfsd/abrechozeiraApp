import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { GridModule, FilterService, PageService, SortService, ToolbarService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ToastModule } from '@syncfusion/ej2-angular-notifications';
import { PEDIDOS_ROUTES } from './pedidos.routes';
import { CadastroPedidoComponent } from './pages/cadastro-pedido/cadastro-pedido.component';

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(PEDIDOS_ROUTES),
    ReactiveFormsModule,
    GridModule,
    ButtonModule,
    TextBoxModule,
    DatePickerModule,
    DropDownListModule,
    ToastModule,
    CadastroPedidoComponent
  ],
  providers: [
    FilterService,
    PageService,
    SortService,
    ToolbarService
  ]
})
export class PedidosModule { } 