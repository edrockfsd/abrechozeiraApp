import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastModule } from '@syncfusion/ej2-angular-notifications';

@Component({
  selector: 'app-relatorio-live-session',
  templateUrl: './relatorio-live-session.component.html',
  styleUrls: ['./relatorio-live-session.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    ButtonModule,
    ToastModule
  ]
})
export class RelatorioLiveSessionComponent implements OnInit {
  constructor() {}
  ngOnInit(): void {}
} 