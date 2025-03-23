import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { PessoaService } from '../../services/pessoa.service';
import { Pessoa } from '../../models/pessoa';
import { ToastService } from '../../../../services/toast.service';

@Component({
  selector: 'app-cadastro-pessoa',
  templateUrl: './cadastro-pessoa.component.html',
  styleUrls: ['./cadastro-pessoa.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    ReactiveFormsModule,
    TextBoxModule,
    DatePickerModule,
    DropDownListModule,
    ButtonModule,
    ToastModule
  ]
})
export class CadastroPessoaComponent implements OnInit {
  @ViewChild('toast') private toastObj: ToastComponent;
  
  pessoaForm: FormGroup;
  isEdicao = false;
  salvando = false;
  categorias: any[] = [];
  tipos: any[] = [];
  generos: any[] = [];
  status: any[] = [];

  public dropDownSettings = {
    placeholder: 'Selecione...',
    allowFiltering: true,
    filterBarPlaceholder: 'Buscar...'
  };

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  constructor(
    private formBuilder: FormBuilder,
    private pessoaService: PessoaService,
    private router: Router,
    private route: ActivatedRoute,
    private toastService: ToastService
  ) {
    this.criarFormulario();
  }

  ngOnInit(): void {
    this.carregarCategorias();
    this.carregarTipos();
    this.carregarGeneros();
    this.carregarStatus();
    
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEdicao = true;
      this.carregarPessoa(id);
    }
  }

  private criarFormulario(): void {
    this.pessoaForm = this.formBuilder.group({
      nome: ['', [Validators.required]],
      dataNascimento: [null, [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      telefone: ['', [Validators.required]],
      pessoaGeneroId: [null, [Validators.required]],
      pessoaCategoriaId: [null, [Validators.required]],
      pessoaTipoId: [null, [Validators.required]],
      statusId: [null, [Validators.required]],
      nickName: ['']
    });
  }

  private carregarCategorias(): void {
    this.pessoaService.listarCategorias().subscribe(
      (categorias) => {
        this.categorias = categorias;
      },
      (erro) => {
        console.error('Erro ao carregar categorias:', erro);
        this.toastService.showError('Erro ao carregar categorias. Por favor, tente novamente.');
      }
    );
  }

  private carregarTipos(): void {
    this.pessoaService.listarTipos().subscribe(
      (tipos) => {
        this.tipos = tipos;
      },
      (erro) => {
        console.error('Erro ao carregar tipos:', erro);
        this.toastService.showError('Erro ao carregar tipos. Por favor, tente novamente.');
      }
    );
  }

  private carregarGeneros(): void {
    this.pessoaService.listarGeneros().subscribe(
      (generos) => {
        this.generos = generos;
      },
      (erro) => {
        console.error('Erro ao carregar gêneros:', erro);
        this.toastService.showError('Erro ao carregar gêneros. Por favor, tente novamente.');
      }
    );
  }

  private carregarStatus(): void {
    this.pessoaService.listarStatus().subscribe(
      (status) => {
        this.status = status;
      },
      (erro) => {
        console.error('Erro ao carregar status:', erro);
        this.toastService.showError('Erro ao carregar status. Por favor, tente novamente.');
      }
    );
  }

  private carregarPessoa(id: number): void {
    this.pessoaService.buscarPorId(id).subscribe(
      (pessoa) => {
        this.pessoaForm.patchValue(pessoa);
      },
      (erro) => {
        console.error('Erro ao carregar pessoa:', erro);
        this.toastService.showError('Erro ao carregar dados da pessoa. Por favor, tente novamente.');
      }
    );
  }

  onSubmit(): void {
    if (this.pessoaForm.valid && !this.salvando) {
      this.salvando = true;
      const pessoa = {
        ...this.pessoaForm.value,
        id: this.isEdicao ? Number(this.route.snapshot.params['id']) : 0,
        dataNascimento: this.formatarData(this.pessoaForm.value.dataNascimento),
        pessoaGeneroId: Number(this.pessoaForm.value.pessoaGeneroId),
        pessoaCategoriaId: Number(this.pessoaForm.value.pessoaCategoriaId),
        pessoaTipoId: Number(this.pessoaForm.value.pessoaTipoId),
        statusId: Number(this.pessoaForm.value.statusId)
      };
      console.log('Pessoa a ser salva:', pessoa);
      console.log('isEdicao:', this.isEdicao);
      console.log('pessoaId:', this.route.snapshot.params['id']);
      const operacao = this.isEdicao
        ? this.pessoaService.atualizar(pessoa.id, pessoa)
        : this.pessoaService.criar(pessoa);

      operacao.subscribe(
        (resultado) => {
          this.toastService.showSuccess(`Pessoa ${this.isEdicao ? 'atualizada' : 'cadastrada'} com sucesso!`);
          this.salvando = false;
        },
        erro => {
          console.error('Erro ao salvar pessoa:', erro);
          this.toastService.showError(`Erro ao ${this.isEdicao ? 'atualizar' : 'cadastrar'} pessoa. Por favor, tente novamente.`);
          this.salvando = false;
        }
      );
    }
  }

  private formatarData(data: any): string {
    if (!data) return '';
    const d = new Date(data);
    return d.toISOString();
  }

  irParaListagem(): void {
    this.router.navigate(['/pessoas']);
  }
} 