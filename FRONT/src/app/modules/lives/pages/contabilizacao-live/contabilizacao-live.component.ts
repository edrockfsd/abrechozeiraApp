import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { ToastComponent } from '@syncfusion/ej2-angular-notifications';

@Component({
  selector: 'app-contabilizacao-live',
  templateUrl: './contabilizacao-live.component.html',
  styleUrls: ['./contabilizacao-live.component.scss']
})
export class ContabilizacaoLiveComponent implements OnInit {
  @ViewChild('grid') public grid!: GridComponent;
  @ViewChild('toast') public toast!: ToastComponent;

  public contabilizacaoForm!: FormGroup;
  public arremates: any[] = [];
  public lives: any[] = [];
  public selectedLiveId: number | null = null;
  
  public toastPosition = { X: 'Right', Y: 'Top' };
  public toastWidth = 300;
  
  public editSettings = {
    allowEditing: false,
    allowDeleting: false,
    allowAdding: false
  };

  public liveFields = { text: 'titulo', value: 'id' };

  constructor(private formBuilder: FormBuilder) {
    this.contabilizacaoForm = this.formBuilder.group({
      liveId: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    // Initialize component
  }


  onActionBegin(args: any): void {
    // Handle grid actions
  }
}