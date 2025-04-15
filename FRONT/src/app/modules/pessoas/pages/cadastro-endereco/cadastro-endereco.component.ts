import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { TipoEnderecoService, TipoEndereco } from '../../services/tipo-endereco.service';
import { EnderecoService, Endereco } from '../../services/endereco.service';

interface ViaCepResponse {
  cep: string;
  logradouro: string;
  complemento: string;
  bairro: string;
  localidade: string;
  uf: string;
  ibge: string;
  gia: string;
  ddd: string;
  siafi: string;
  erro?: boolean;
}

@Component({
  selector: 'app-cadastro-endereco',
  templateUrl: './cadastro-endereco.component.html',
  styleUrls: ['./cadastro-endereco.component.scss'],
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
export class CadastroEnderecoComponent implements OnInit {
  @ViewChild('toast') public toast!: ToastComponent;
  
  public enderecoForm!: FormGroup;
  public isEditMode = false;
  public pessoaId!: number;
  public enderecoId?: number;
  public salvando = false;
  public buscandoCep = false;
  public carregandoTipos = false;
  
  public tiposEndereco: TipoEndereco[] = [];
  
  public estados = [
    { sigla: 'AC', nome: 'Acre' },
    { sigla: 'AL', nome: 'Alagoas' },
    { sigla: 'AP', nome: 'Amapá' },
    { sigla: 'AM', nome: 'Amazonas' },
    { sigla: 'BA', nome: 'Bahia' },
    { sigla: 'CE', nome: 'Ceará' },
    { sigla: 'DF', nome: 'Distrito Federal' },
    { sigla: 'ES', nome: 'Espírito Santo' },
    { sigla: 'GO', nome: 'Goiás' },
    { sigla: 'MA', nome: 'Maranhão' },
    { sigla: 'MT', nome: 'Mato Grosso' },
    { sigla: 'MS', nome: 'Mato Grosso do Sul' },
    { sigla: 'MG', nome: 'Minas Gerais' },
    { sigla: 'PA', nome: 'Pará' },
    { sigla: 'PB', nome: 'Paraíba' },
    { sigla: 'PR', nome: 'Paraná' },
    { sigla: 'PE', nome: 'Pernambuco' },
    { sigla: 'PI', nome: 'Piauí' },
    { sigla: 'RJ', nome: 'Rio de Janeiro' },
    { sigla: 'RN', nome: 'Rio Grande do Norte' },
    { sigla: 'RS', nome: 'Rio Grande do Sul' },
    { sigla: 'RO', nome: 'Rondônia' },
    { sigla: 'RR', nome: 'Roraima' },
    { sigla: 'SC', nome: 'Santa Catarina' },
    { sigla: 'SP', nome: 'São Paulo' },
    { sigla: 'SE', nome: 'Sergipe' },
    { sigla: 'TO', nome: 'Tocantins' }
  ];

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  public dropDownSettings = {
    placeholder: 'Selecione...',
    allowFiltering: true,
    filterBarPlaceholder: 'Buscar...'
  };

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private tipoEnderecoService: TipoEnderecoService,
    private enderecoService: EnderecoService
  ) {}

  ngOnInit(): void {
    this.initForm();
    
    this.route.params.subscribe(params => {
      this.pessoaId = +params['pessoaId'];
      if (params['id']) {
        this.enderecoId = +params['id'];
        this.isEditMode = true;
      }
    });

    // Primeiro carrega os tipos de endereço
    this.carregarTiposEndereco();
  }

  private initForm(): void {
    this.enderecoForm = this.formBuilder.group({
      tipoEndereco: ['', Validators.required],
      cep: ['', [Validators.required, Validators.pattern(/^\d{8}$/)]],
      logradouro: ['', Validators.required],
      numero: ['', Validators.required],
      complemento: [''],
      bairro: ['', Validators.required],
      cidade: ['', Validators.required],
      estado: ['', Validators.required],
      ibge: [''],
      observacoes: ['']
    });
  }

  private carregarTiposEndereco(): void {
    this.carregandoTipos = true;
    this.tipoEnderecoService.listar().subscribe({
      next: (tipos) => {
        this.tiposEndereco = tipos;
        this.carregandoTipos = false;
        
        // Se estiver em modo de edição, carrega o endereço após carregar os tipos
        if (this.isEditMode) {
          this.loadEndereco();
        }
      },
      error: (error) => {
        console.error('Erro ao carregar tipos de endereço:', error);
        this.toast.show({
          title: 'Erro',
          content: 'Erro ao carregar tipos de endereço. Tente novamente.',
          cssClass: 'e-toast-danger',
          icon: 'e-error toast-icons'
        });
        this.carregandoTipos = false;
      }
    });
  }

  buscarCep(): void {
    const cep = this.enderecoForm.get('cep')?.value?.replace(/\D/g, '');
    
    if (cep && cep.length === 8) {
      this.buscandoCep = true;
      
      this.http.get<ViaCepResponse>(`https://viacep.com.br/ws/${cep}/json/`)
        .subscribe({
          next: (data) => {
            if (!data.erro) {
              this.enderecoForm.patchValue({
                logradouro: data.logradouro,
                bairro: data.bairro,
                cidade: data.localidade,
                estado: data.uf,
                ibge: data.ibge
              });
              
              // Se o complemento estiver vazio, preencher com o valor retornado
              if (!this.enderecoForm.get('complemento')?.value && data.complemento) {
                this.enderecoForm.patchValue({
                  complemento: data.complemento
                });
              }
              
              this.toast.show({
                title: 'Sucesso',
                content: 'CEP encontrado com sucesso!',
                cssClass: 'e-toast-success',
                icon: 'e-success toast-icons'
              });
            } else {
              this.toast.show({
                title: 'Atenção',
                content: 'CEP não encontrado.',
                cssClass: 'e-toast-warning',
                icon: 'e-warning toast-icons'
              });
            }
            this.buscandoCep = false;
          },
          error: (error) => {
            console.error('Erro ao buscar CEP:', error);
            this.toast.show({
              title: 'Erro',
              content: 'Erro ao buscar CEP. Tente novamente.',
              cssClass: 'e-toast-danger',
              icon: 'e-error toast-icons'
            });
            this.buscandoCep = false;
          }
        });
    }
  }

  loadEndereco(): void {
    if (this.enderecoId) {
      this.enderecoService.obterPorId(this.enderecoId).subscribe({
        next: (endereco) => {
          // Encontrar o tipo de endereço correspondente
          const tipoEndereco = this.tiposEndereco.find(tipo => tipo.id === endereco.tipoEnderecoId);          
          
          this.enderecoForm.patchValue({
            tipoEndereco: tipoEndereco?.id || '',
            cep: endereco.cep,
            logradouro: endereco.logradouro,
            numero: endereco.unidade,
            complemento: endereco.complemento,
            bairro: endereco.bairro,
            cidade: endereco.localidade,
            estado: endereco.estado,
            ibge: endereco.codigoLocalidadeIBGE,
            observacoes: endereco.observacoes || ''
          });
        },
        error: (error) => {
          console.error('Erro ao carregar endereço:', error);
          this.toast.show({
            title: 'Erro',
            content: 'Erro ao carregar endereço. Tente novamente.',
            cssClass: 'e-toast-danger',
            icon: 'e-error toast-icons'
          });
        }
      });
    }
  }

  onSubmit(): void {
    if (this.enderecoForm.valid && !this.salvando) {
      this.salvando = true;
      
      const endereco: Endereco = {
        id: this.isEditMode ? this.enderecoId : undefined,
        pessoaId: this.pessoaId,
        tipoEnderecoId: this.enderecoForm.get('tipoEndereco')?.value,
        cep: this.enderecoForm.get('cep')?.value?.replace(/\D/g, ''),
        logradouro: this.enderecoForm.get('logradouro')?.value,
        unidade: this.enderecoForm.get('numero')?.value,
        complemento: this.enderecoForm.get('complemento')?.value || '',
        bairro: this.enderecoForm.get('bairro')?.value,
        localidade: this.enderecoForm.get('cidade')?.value,
        codigoLocalidadeIBGE: this.enderecoForm.get('ibge')?.value || '',
        estado: this.enderecoForm.get('estado')?.value,
        observacoes: this.enderecoForm.get('observacoes')?.value || '',
        usuarioModificacaoId: 1 // Temporário até o módulo de usuários estar pronto
      };

      console.log('Endereço a ser salvo:', endereco);
      
      const operacao = this.isEditMode
        ? this.enderecoService.atualizar(this.enderecoId!, endereco)
        : this.enderecoService.criar(endereco);

      operacao.subscribe({
        next: () => {
          this.toast.show({
            title: 'Sucesso',
            content: `Endereço ${this.isEditMode ? 'atualizado' : 'cadastrado'} com sucesso!`,
            cssClass: 'e-toast-success',
            icon: 'e-success toast-icons'
          });
          //this.router.navigate(['/pessoas', this.pessoaId, 'endereco']);
        },
        error: (error) => {
          console.error('Erro ao salvar endereço:', error);
          this.toast.show({
            title: 'Erro',
            content: 'Erro ao salvar endereço. Tente novamente.',
            cssClass: 'e-toast-danger',
            icon: 'e-error toast-icons'
          });
          this.salvando = false;
        }
      });
    } else {
      this.enderecoForm.markAllAsTouched();
      this.toast.show({
        title: 'Atenção',
        content: 'Por favor, preencha todos os campos obrigatórios.',
        cssClass: 'e-toast-warning',
        icon: 'e-warning toast-icons'
      });
    }
  }

  onCancelar(): void {
    this.router.navigate(['/pessoas']);
  }

  onVoltar(): void {
    this.router.navigate(['/pessoas', this.pessoaId, 'endereco']);
  }
}