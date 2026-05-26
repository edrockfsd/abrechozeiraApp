import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { PdvRoutingModule } from './pdv-routing.module';
import { PdvShellComponent } from './pages/pdv-shell/pdv-shell.component';

@NgModule({
  declarations: [],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PdvShellComponent, PdvRoutingModule]
})
export class PdvModule {}
