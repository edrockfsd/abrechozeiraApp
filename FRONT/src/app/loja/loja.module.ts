import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NbMenuModule } from '@nebular/theme';

import { ThemeModule } from '../@theme/theme.module';



@NgModule({
  imports: [
    CommonModule,
    ThemeModule,
    NbMenuModule
  ],
  declarations: [
  ]
})
export class LojaModule { }
