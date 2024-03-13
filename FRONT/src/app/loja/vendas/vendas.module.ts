import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NbMenuModule } from '@nebular/theme';
import { NbCardModule, NbIconModule, NbInputModule } from '@nebular/theme';
import { Ng2SmartTableModule } from 'ng2-smart-table';

import { ThemeModule } from '../../@theme/theme.module';

import { VendasRoutingModule } from './vendas-routing.module';
import { VendasComponent } from './vendas.component';
import { ListarComponent } from './listar/listar.component';



@NgModule({
  imports: [
    CommonModule,
    ThemeModule,
    NbMenuModule,
    NbCardModule,
    NbIconModule,
    NbInputModule,
    Ng2SmartTableModule,
    VendasRoutingModule
  ],
  declarations: [VendasComponent, ListarComponent]
})
export class VendasModule { }
