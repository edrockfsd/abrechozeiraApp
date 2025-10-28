import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { NumericTextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { Produto, ProdutoStatus } from '../../models/produto';
import { ProdutoService } from '../../services/produto.service';
import { ProdutoGrupoService } from '../../services/produto-grupo.service';
import { ProdutoMarcaService } from '../../services/produto-marca.service';
import { PessoaGeneroService } from '../../services/pessoa-genero.service';
import { PessoaPerfilService } from '../../services/pessoa-perfil.service';
import { ProdutoGrupo } from '../../models/produto-grupo';
import { ProdutoMarca } from '../../models/produto-marca';
import { PessoaGenero } from '../../models/pessoa-genero';
import { PessoaPerfil } from '../../models/pessoa-perfil';
import { EstoqueService } from '../../../estoque/services/estoque.service';
import { ToastService } from '../../../../services/toast.service';


@Component({
  selector: 'app-cadastro-produto',
  templateUrl: './cadastro-produto.component.html',
  styleUrls: ['./cadastro-produto.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    ReactiveFormsModule,
    TextBoxModule,
    DatePickerModule,
    DropDownListModule,
    ButtonModule,
    NumericTextBoxModule,
    ToastModule
  ]
})
export class CadastroProdutoComponent implements OnInit {
  @ViewChild('toast') private toastObj: ToastComponent;
  
  produtoForm: FormGroup;
  isEdicao = false;
  produtoId: number;
  salvando = false;
  
  // Configurações dos componentes
  public dropDownSettings = {
    placeholder: 'Selecione...',
    mode: 'Default',
    allowFiltering: true,
    filterBarPlaceholder: 'Buscar...'
  };

  public datePickerSettings = {
    placeholder: 'Selecione uma data',
    format: 'dd/MM/yyyy'
  };

  public numericSettings = {
    format: 'c2',
    currency: 'BRL',
    decimals: 2,
    validateDecimalOnType: true,
    min: 0
  };

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };
  
  grupos: ProdutoGrupo[] = [];
  marcas: ProdutoMarca[] = [];
  generos: PessoaGenero[] = [];
  perfis: PessoaPerfil[] = [];
  
  constructor(
    private fb: FormBuilder,
    private produtoService: ProdutoService,
    private produtoGrupoService: ProdutoGrupoService,
    private produtoMarcaService: ProdutoMarcaService,
    private pessoaGeneroService: PessoaGeneroService,
    private pessoaPerfilService: PessoaPerfilService,
    private route: ActivatedRoute,
    private router: Router,
    private estoqueService: EstoqueService,
    private toastService: ToastService
  ) {
    this.criarFormulario();
  }

  ngOnInit(): void {
    this.carregarDados();
  }

  ngAfterViewInit() {
    if (this.toastObj) {
      this.toastService.setToastComponent(this.toastObj);
    }
  }

  private criarFormulario(): void {
    this.produtoForm = this.fb.group({
      descricao: ['', [Validators.required, Validators.maxLength(100)]],
      tamanho: ['', [Validators.required, Validators.maxLength(50)]],
      precoCusto: [0, [Validators.required, Validators.min(0)]],
      precoVenda: [0, [Validators.required, Validators.min(0)]],
      origem: ['', [Validators.required, Validators.maxLength(50)]],
      grupoId: [null, Validators.required],
      dataCompra: [new Date(), Validators.required],
      statusId: [ProdutoStatus.Ativo, Validators.required],
      marcaId: [null, Validators.required],
      generoId: [null, Validators.required],
      perfilId: [null, Validators.required]
    });
  }

  private carregarGrupos(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.produtoGrupoService.listar().subscribe(
        grupos => {
          if (!grupos || grupos.length === 0) {
            console.error('Nenhum grupo foi retornado do servidor');
            reject(new Error('Nenhum grupo disponível'));
            return;
          }
          this.grupos = grupos;
          console.log('Grupos carregados com sucesso:', grupos.map(g => ({id: g.id, descricao: g.descricao})));
          resolve();
        },
        erro => {
          console.error('Erro ao carregar grupos:', erro);
          reject(erro);
        }
      );
    });
  }

  private carregarMarcas(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.produtoMarcaService.listar().subscribe(
        marcas => {
          if (!marcas || marcas.length === 0) {
            console.error('Nenhuma marca foi retornada do servidor');
            reject(new Error('Nenhuma marca disponível'));
            return;
          }
          this.marcas = marcas;
          console.log('Marcas carregadas com sucesso:', marcas.map(m => ({id: m.id, descricao: m.descricao})));
          resolve();
        },
        erro => {
          console.error('Erro ao carregar marcas:', erro);
          reject(erro);
        }
      );
    });
  }

  private carregarGeneros(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.pessoaGeneroService.listar().subscribe(
        generos => {
          if (!generos || generos.length === 0) {
            console.error('Nenhum gênero foi retornado do servidor');
            reject(new Error('Nenhum gênero disponível'));
            return;
          }
          this.generos = generos;
          console.log('Gêneros carregados com sucesso:', generos.map(g => ({id: g.id, descricao: g.descricao})));
          resolve();
        },
        erro => {
          console.error('Erro ao carregar gêneros:', erro);
          reject(erro);
        }
      );
    });
  }

  private carregarPerfis(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.pessoaPerfilService.listar().subscribe(
        perfis => {
          if (!perfis || perfis.length === 0) {
            console.error('Nenhum perfil foi retornado do servidor');
            reject(new Error('Nenhum perfil disponível'));
            return;
          }
          this.perfis = perfis;
          console.log('Perfis carregados com sucesso:', perfis.map(p => ({id: p.id, descricao: p.descricao})));
          resolve();
        },
        erro => {
          console.error('Erro ao carregar perfis:', erro);
          reject(erro);
        }
      );
    });
  }

  private carregarProduto(): void {
    this.produtoService.buscarPorId(this.produtoId).subscribe(
      produto => {
        console.log('Produto recebido:', produto);
        console.log('Produto generoId:', produto.generoID);        
        console.log('Produto grupoId:', produto.grupoID);
        console.log('Produto perfilId:', produto.perfilID);
        console.log('Produto marcaId:', produto.marcaId);

        // Recarregar os dados relacionados antes de preencher o formulário
        Promise.all([
          this.carregarGrupos().catch(erro => {
            console.error('Falha ao carregar grupos:', erro);
            this.toastService.showError('Erro ao carregar grupos. Verifique se existem grupos cadastrados.');
            return Promise.reject(erro);
          }),
          this.carregarGeneros().catch(erro => {
            console.error('Falha ao carregar gêneros:', erro);
            this.toastService.showError('Erro ao carregar gêneros. Verifique se existem gêneros cadastrados.');
            return Promise.reject(erro);
          }),
          this.carregarPerfis().catch(erro => {
            console.error('Falha ao carregar perfis:', erro);
            this.toastService.showError('Erro ao carregar perfis. Verifique se existem perfis cadastrados.');
            return Promise.reject(erro);
          }),
          this.carregarMarcas().catch(erro => {
            console.error('Falha ao carregar marcas:', erro);
            this.toastService.showError('Erro ao carregar marcas. Verifique se existem marcas cadastradas.');
            return Promise.reject(erro);
          })
        ]).then(() => {
          // Garantimos que os campos de ID estejam como números
          const produtoFormatado = {
            ...produto,
            grupoId: produto.grupoID ? Number(produto.grupoID) : null,
            generoId: produto.generoID ? Number(produto.generoID) : null,
            perfilId: produto.perfilID ? Number(produto.perfilID) : null,
            marcaId: produto.marcaId ? Number(produto.marcaId) : null,
            dataCompra: produto.dataCompra ? new Date(produto.dataCompra) : null
          };

          console.log('Produto formatado:', produtoFormatado);
          
          // Verificamos se os IDs existem nas listas carregadas
          const grupoExiste = this.grupos.some(g => g.id === produtoFormatado.grupoId);
          const generoExiste = this.generos.some(g => g.id === produtoFormatado.generoId);
          const perfilExiste = this.perfis.some(p => p.id === produtoFormatado.perfilId);
          
          if (!grupoExiste) {
            console.error(`Grupo com ID ${produtoFormatado.grupoId} não encontrado nos grupos disponíveis:`, this.grupos);
          }
          if (!generoExiste) {
            console.error(`Gênero com ID ${produtoFormatado.generoId} não encontrado nos gêneros disponíveis:`, this.generos);
          }
          if (!perfilExiste) {
            console.error(`Perfil com ID ${produtoFormatado.perfilId} não encontrado nos perfis disponíveis:`, this.perfis);
          }

          // Atualiza o formulário com todos os valores de uma vez
          this.produtoForm.patchValue(produtoFormatado);
          
          console.log('Estado final do formulário:', this.produtoForm.value);
          
          if (!grupoExiste || !generoExiste || !perfilExiste) {
            this.toastService.showWarning('Alguns dados relacionados ao produto não foram encontrados. Verifique se todos os cadastros necessários existem.');
          }
        }).catch(erro => {
          console.error('Erro ao carregar dados relacionados:', erro);
          this.toastService.showError('Erro ao carregar dados necessários. Por favor, verifique se todos os cadastros básicos existem.');
        });
      },
      erro => {
        console.error('Erro ao carregar produto:', erro);
        this.toastService.showError('Erro ao carregar produto. Tente novamente.');
      }
    );
  }

  onSubmit(): void {
    if (this.produtoForm.valid && !this.salvando) {
      this.salvando = true;
      const formValues = this.produtoForm.value;
      
      // Garantir que os IDs sejam números
      const produto: Produto = {
        ...formValues,
        grupoId: Number(formValues.grupoId),
        generoId: Number(formValues.generoId),
        perfilId: Number(formValues.perfilId),
        marcaId: Number(formValues.marcaId),
        statusId: Number(formValues.statusId),
        // Se estiver em modo de edição, inclui o ID
        ...(this.isEdicao && { id: this.produtoId })
      };

      console.log('Produto formatado para envio:', produto);

      const operacao = this.isEdicao
        ? this.produtoService.atualizar(this.produtoId, produto)
        : this.produtoService.criar(produto);

      operacao.subscribe(
        (resultado) => {
          console.log('Resposta da API:', resultado);
          this.toastService.showSuccess(`Produto ${this.isEdicao ? 'atualizado' : 'cadastrado'} com sucesso.`);
          
          if (!this.isEdicao) {
            // Criar estoque inicial para o novo produto
            this.estoqueService.criarEstoqueInicial(resultado.id).subscribe(
              (estoqueResultado) => {
                console.log('Estoque inicial criado com sucesso:', estoqueResultado);
                this.toastService.showSuccess('Produto e estoque inicial criados com sucesso!');
                // Limpar o formulário para um novo cadastro
                this.produtoForm.reset();
                this.produtoForm.patchValue({
                  statusId: ProdutoStatus.Ativo,
                  dataCompra: new Date(),
                  precoCusto: 0,
                  precoVenda: 0
                });
              },
              (erro) => {
                console.error('Erro ao criar estoque inicial:', erro);
                this.toastService.showWarning('Produto criado com sucesso, mas houve um erro ao criar o estoque inicial.');
              }
            );
          } else {
            this.router.navigate(['/produtos']);
          }
          
          this.salvando = false;
        },
        erro => {
          console.error('Erro ao salvar produto:', erro);
          console.error('Detalhes do erro:', {
            status: erro.status,
            statusText: erro.statusText,
            error: erro.error
          });
          this.toastService.showError(`Erro ao ${this.isEdicao ? 'atualizar' : 'cadastrar'} produto. Por favor, tente novamente.`);
          this.salvando = false;
        }
      );
    } else {
      Object.keys(this.produtoForm.controls).forEach(key => {
        const control = this.produtoForm.get(key);
        if (control?.invalid) {
          control.markAsTouched();
          console.log(`Campo inválido: ${key}`, control.errors);
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/produtos']);
  }

  irParaListagem(): void {
    this.router.navigate(['/produtos']);
  }

  private async carregarDados() {
    try {
      await Promise.all([
        this.carregarGrupos(),
        this.carregarMarcas(),
        this.carregarGeneros(),
        this.carregarPerfis()
      ]);

      const id = this.route.snapshot.params['id'];
      if (id) {
        this.isEdicao = true;
        this.produtoId = id;
        await this.carregarProduto();
      }
    } catch (erro) {
      console.error('Erro ao carregar dados:', erro);
      this.toastService.showError('Erro ao carregar dados. Por favor, tente novamente.');
    }
  }
} 