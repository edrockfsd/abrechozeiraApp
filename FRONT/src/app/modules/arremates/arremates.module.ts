import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { ComboBoxModule } from '@syncfusion/ej2-angular-dropdowns';
import { ArremateComponent } from './pages/arremate/arremate.component';
import { ImportarLiveComponent } from './pages/importar-live/importar-live.component';
import { ARREMATES_ROUTES } from './arremates.routes';
import { 
  GridModule, 
  FilterService, 
  SortService, 
  PageService, 
  ToolbarService,
  EditService
} from '@syncfusion/ej2-angular-grids';

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(ARREMATES_ROUTES),
    ReactiveFormsModule,
    ComboBoxModule,
    GridModule,
    ArremateComponent,
    ImportarLiveComponent
  ],
  providers: [
    FilterService,
    SortService,
    PageService,
    ToolbarService,
    EditService
  ]
})
export class ArrematesModule { }