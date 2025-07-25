import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { GridModule, FilterService, PageService, SortService, ToolbarService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';

import { ToastModule } from '@syncfusion/ej2-angular-notifications';
import { LIVSESSIONS_ROUTES } from './../live-sessions/live-sessions.routes';
import { MatCardModule } from '@angular/material/card';

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(LIVSESSIONS_ROUTES),
    ReactiveFormsModule,
    GridModule,
    ButtonModule,    
    ToastModule,    
    MatCardModule
    // Componentes serão adicionados posteriormente
  ],
  providers: [
    FilterService,
    PageService,
    SortService,
    ToolbarService
  ]
})
export class LiveSessionsModule { } 