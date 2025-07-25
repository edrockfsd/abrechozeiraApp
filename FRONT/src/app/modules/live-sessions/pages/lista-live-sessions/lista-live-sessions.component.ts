import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastModule } from '@syncfusion/ej2-angular-notifications';

@Component({
  selector: 'app-lista-live-sessions',
  templateUrl: './lista-live-sessions.component.html',
  styleUrls: ['./lista-live-sessions.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    GridModule,
    ButtonModule,
    ToastModule
  ]
})
export class ListaLiveSessionsComponent implements OnInit {
  constructor() {}
  ngOnInit(): void {}
} 