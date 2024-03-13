import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NbMenuModule } from '@nebular/theme';
import { NbCardModule, NbIconModule, NbInputModule } from '@nebular/theme';
import { Ng2SmartTableModule } from 'ng2-smart-table';

import { ThemeModule } from '../../@theme/theme.module';

import { ClientesRoutingModule } from './clientes-routing.module';
import { ClientesComponent } from './clientes.component';


@NgModule({
  imports: [
    CommonModule,
    ThemeModule,
    NbMenuModule,
    NbCardModule,
    NbIconModule,
    NbInputModule,
    Ng2SmartTableModule,
    ClientesRoutingModule
  ],
  declarations: [ClientesComponent]
})
export class ClientesModule { }
