import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DateTimePickerModule } from '@syncfusion/ej2-angular-calendars';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { LiveService } from '../../services/live.service';
import { Live, LiveCreate } from '../../models/live';
import { ToastService } from '../../../../services/toast.service';

@Component({
  selector: 'app-cadastro-live',
  templateUrl: './cadastro-live.component.html',
  styleUrls: ['./cadastro-live.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    TextBoxModule,
    DateTimePickerModule,
    ToastModule
  ]
})
export class CadastroLiveComponent implements OnInit {
  @ViewChild('toast') private toastObj: ToastComponent;

  liveForm: FormGroup;
  isEdicao = false;
  liveId: number;

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  constructor(
    private formBuilder: FormBuilder,
    private liveService: LiveService,
    private router: Router,
    private route: ActivatedRoute,
    private toastService: ToastService
  ) {
    this.criarFormulario();
  }

  ngOnInit(): void {
    this.liveId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.liveId) {
      this.isEdicao = true;
      this.carregarLive();
    }
  }

  ngAfterViewInit(): void {
    if (this.toastObj) {
      this.toastService.setToastComponent(this.toastObj);
    }
  }

  private criarFormulario(): void {
    this.liveForm = this.formBuilder.group({
      titulo: ['', [Validators.required]],
      observacoes: [''],
      dataLive: ['', [Validators.required]],
      usuarioModificacaoId: [1] // Valor fixo para teste
    });
  }

  private carregarLive(): void {
    this.liveService.buscarPorId(this.liveId).subscribe(
      (live) => {
        this.liveForm.patchValue({
          titulo: live.titulo,
          observacoes: live.observacoes,
          dataLive: new Date(live.dataLive),
          usuarioModificacaoId: live.usuarioModificacaoId
        });
      },
      (erro) => {
        console.error('Erro ao carregar live:', erro);
        this.toastService.showError('Erro ao carregar live. Por favor, tente novamente.');
      }
    );
  }

  onSubmit(): void {
    if (this.liveForm.valid) {
      const formData = this.liveForm.value;
      
      if (this.isEdicao) {
        const liveUpdate: Live = {
          id: this.liveId,
          ...formData,
          dataAlteracao: new Date().toISOString()
        };
        
        this.liveService.atualizar(this.liveId, liveUpdate).subscribe(
          () => {
            this.toastService.showSuccess('Live atualizada com sucesso!');
            //this.router.navigate(['/lives']);
          },
          (erro) => {
            console.error('Erro ao atualizar live:', erro);
            this.toastService.showError('Erro ao atualizar live. Por favor, tente novamente.');
          }
        );
      } else {
        const liveCreate: LiveCreate = formData;
        this.liveService.criar(liveCreate).subscribe(
          () => {
            this.toastService.showSuccess('Live cadastrada com sucesso!');
            //this.router.navigate(['/lives']);
          },
          (erro) => {
            console.error('Erro ao cadastrar live:', erro);
            this.toastService.showError('Erro ao cadastrar live. Por favor, tente novamente.');
          }
        );
      }
    }
  }

  onCancelar(): void {
    this.router.navigate(['/lives']);
  }
} 