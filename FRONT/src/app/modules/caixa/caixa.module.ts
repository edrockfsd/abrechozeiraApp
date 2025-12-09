import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CaixaRoutingModule } from './caixa-routing.module';
import { CaixaPainelComponent } from './pages/caixa-painel/caixa-painel.component';

@NgModule({
  imports: [
    CommonModule,
    CaixaRoutingModule,
    CaixaPainelComponent
  ]
})
export class CaixaModule { }

