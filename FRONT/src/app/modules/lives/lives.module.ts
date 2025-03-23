import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LIVES_ROUTES } from './lives.routes';
import { ListaLivesComponent } from './pages/lista-lives/lista-lives.component';
import { CadastroLiveComponent } from './pages/cadastro-live/cadastro-live.component';
import { 
  GridModule, 
  FilterService,
  PageService,
  SortService,
  ToolbarService
} from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DateTimePickerModule } from '@syncfusion/ej2-angular-calendars';
import { ToastModule } from '@syncfusion/ej2-angular-notifications';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(LIVES_ROUTES),
    GridModule,
    ButtonModule,
    TextBoxModule,
    DateTimePickerModule,
    ToastModule,
    ReactiveFormsModule,
    ListaLivesComponent,
    CadastroLiveComponent
  ],
  providers: [
    FilterService,
    PageService,
    SortService,
    ToolbarService
  ]
})
export class LivesModule { } 