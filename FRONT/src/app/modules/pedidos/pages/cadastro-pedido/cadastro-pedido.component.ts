import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { L10n, loadCldr, setCulture, setCurrencyCode } from '@syncfusion/ej2-base';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule, RadioButtonModule } from '@syncfusion/ej2-angular-buttons';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ToastModule, ToastComponent } from '@syncfusion/ej2-angular-notifications';
import { NumericTextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { PessoaService } from '../../../pessoas/services/pessoa.service';
import { EnderecoService } from '../../../pessoas/services/endereco.service';
import { CondicaoPagamentoService, CondicaoPagamento } from '../../services/condicao-pagamento.service';
import { FormaPagamentoService, FormaPagamento } from '../../services/forma-pagamento.service';
import { EstoqueService, ProdutoEstoque } from '../../services/estoque.service';
import { PedidoService, Pedido } from '../../services/pedido.service';
import { PedidoStatusService, PedidoStatus } from '../../services/pedido-status.service';
import { EditService, ToolbarService } from '@syncfusion/ej2-angular-grids';
import { PedidoProdutoService, PedidoProduto } from '../../services/pedido-produto.service';

import * as cagregorian from "../../../../shared/ca-gregorian.json";
import * as currencies from "../../../../shared/currencies.json";
import * as numbers from "../../../../shared/numbers.json";
import * as timeZoneNames from "../../../../shared/timeZoneNames.json";
import * as numberingSystems from "../../../../shared/numberingSystmes.json"

setCulture('pt');
setCurrencyCode('BRL');
loadCldr(numberingSystems['default'],cagregorian['default'],currencies['default'], numbers['default'], timeZoneNames['default']); 

interface Produto {
  id: number;
  produtoId: string;
  codigoEstoque: string;
  descricao: string;
  quantidade: number;
  preco: number;
  desconto: number;
  valorFinal: number;
}

interface Endereco {
  id?: number;
  pessoaId: number;
  tipoEnderecoId: number;
  cep: string;
  logradouro: string;
  unidade: string;
  complemento?: string;
  bairro: string;
  localidade: string;
  estado: string;
  codigoLocalidadeIBGE?: string;
  numeroComplemento?: string;
}

@Component({
  selector: 'app-cadastro-pedido',
  templateUrl: './cadastro-pedido.component.html',
  styleUrls: ['./cadastro-pedido.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule,
    GridModule,
    ButtonModule,
    TextBoxModule,
    DatePickerModule,
    DropDownListModule,
    ToastModule,
    NumericTextBoxModule,
    RadioButtonModule
],
  providers: [EditService, ToolbarService]
})
export class CadastroPedidoComponent implements OnInit {
  public pedidoForm!: FormGroup;
  public clientes: any[] = [];
  public carregandoClientes = false;
  public codigoProduto: string = '';
  public codigoEstoque: string = '';
  public descricaoProduto: string = '';
  public quantidade: number = 1;
  public percentualDesconto: number = 0;
  public valorDesconto: number = 0;
  public valorUnitario: number = 0;
  public produtos: Produto[] = [];
  public condicoesPagamento: CondicaoPagamento[] = [];
  public formasPagamento: FormaPagamento[] = [];
  public enderecos: Endereco[] = [];
  public carregandoCondicoesPagamento = false;
  public carregandoFormasPagamento = false;
  public carregandoProduto = false;
  @ViewChild('toast', { static: false }) toastObj!: ToastComponent;
  public salvando = false;
  public statusPedido: PedidoStatus[] = [];
  public carregandoStatus = false;
  public pedidoSalvo = false;
  private salvamentoEmAndamento = false; // Variável para rastrear operações de salvamento

  get camposBloqueados(): boolean {
    return !this.pedidoForm.get('numeroPedido')?.value;
  }

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private pessoaService: PessoaService,
    private enderecoService: EnderecoService,
    private condicaoPagamentoService: CondicaoPagamentoService,
    private formaPagamentoService: FormaPagamentoService,
    private estoqueService: EstoqueService,
    private pedidoService: PedidoService,
    private pedidoStatusService: PedidoStatusService,
    private pedidoProdutoService: PedidoProdutoService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.carregarClientes();
    this.carregarCondicoesPagamento();
    this.carregarFormasPagamento();
    this.carregarStatusPedido();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.carregarPedido(Number(id));
    }
  }

  private initForm(): void {
    this.pedidoForm = this.formBuilder.group({
      id: [null],
      numeroPedido: [{ value: '', disabled: true }],
      dataLancamento: [{ value: new Date(), disabled: true }],
      horaLancamento: [{ value: this.getHoraAtual(), disabled: true }],
      statusPedidoId: [1, Validators.required],
      clienteId: [null, Validators.required],
      pessoaId: [null, Validators.required],
      enderecoId: [null],
      descontoGeral: [{ value: 0, disabled: true }],
      valorFrete: [{ value: 0, disabled: true }],
      valorTotal: [{ value: 0, disabled: true }],
      condicaoPagamento: [{ value: null, disabled: true }],
      formaPagamento: [{ value: null, disabled: true }],
      observacao: [{ value: '', disabled: true }],      
      usuarioModificacaoId: [null]
    });

    // Observa mudanças no clienteId para carregar os endereços
    this.pedidoForm.get('clienteId')?.valueChanges.subscribe(clienteId => {
      this.pedidoForm.patchValue({ pessoaId: clienteId }, { emitEvent: false });
      if (clienteId) {
        this.carregarEnderecos(clienteId);
      } else {
        this.enderecos = [];
      }
    });

    // Observa mudanças no valorFrete para recalcular os totais
    this.pedidoForm.get('valorFrete')?.valueChanges.subscribe(() => {
      this.atualizarTotais();
    });
  }

  private carregarEnderecos(clienteId: number): void {
    // TODO: Implementar chamada à API quando estiver disponível
    this.enderecoService.listarPorPessoa(clienteId).subscribe({
      next: (enderecos) => {
        this.enderecos = enderecos.map(e => ({
          ...e,
          numeroComplemento: `${e.unidade}${e.complemento ? ' - ' + e.complemento : ''}`
        }));
      },
      error: (error) => {
        console.error('Erro ao carregar endereços:', error);
      }
    });
  }

  onEnderecoSelecionado(enderecoId: number): void {
    this.pedidoForm.patchValue({ enderecoId: enderecoId });
  }

  private carregarClientes(): void {
    this.carregandoClientes = true;
    this.pessoaService.listarClientesCombo().subscribe({
      next: (clientes) => {
        this.clientes = clientes;
        this.carregandoClientes = false;
      },
      error: (error) => {
        console.error('Erro ao carregar clientes:', error);
        this.carregandoClientes = false;
      }
    });
  }

  private getHoraAtual(): string {
    const agora = new Date();
    const horas = String(agora.getHours()).padStart(2, '0');
    const minutos = String(agora.getMinutes()).padStart(2, '0');
    return `${horas}:${minutos}`;
  }

  onSubmit(): void {
    if (this.pedidoForm.valid) {
      console.log('Formulário válido:', this.pedidoForm.value);
    } else {
      this.pedidoForm.markAllAsTouched();
    }
  }

  onAdicionarProduto(): void {
    if (!this.codigoEstoque || !this.pedidoForm.get('id')?.value) {
      this.toastObj.show({
        title: 'Atenção',
        content: 'Informe o código de estoque e salve os dados básicos do pedido antes de adicionar produtos.',
        cssClass: 'e-warning',
        timeOut: 3000
      });
      return;
    }

    const produto: any = {
      pedidoId: this.pedidoForm.get('id')?.value,
      produtoId: Number(this.codigoProduto),
      codigoEstoque: this.codigoEstoque,
      descricao: this.descricaoProduto,
      quantidade: this.quantidade,
      valorUnitario: this.valorUnitario,
      descontoValor: this.valorDesconto,
      valorFinalProduto: (this.valorUnitario * this.quantidade) - this.valorDesconto,
      dataAlteracao: new Date(),
      usuarioModificacaoId: 1 // TODO: pegar do auth
    };

    this.pedidoProdutoService.adicionar(produto).subscribe({
      next: () => {
        this.toastObj.show({
          title: 'Sucesso',
          content: 'Produto adicionado ao pedido!',
          cssClass: 'e-success',
          timeOut: 3000
        });
        this.limparCamposProduto();
        this.atualizarGridProdutos();
      },
      error: () => {
        this.toastObj.show({
          title: 'Erro',
          content: 'Erro ao adicionar produto ao pedido.',
          cssClass: 'e-error',
          timeOut: 3000
        });
      }
    });
  }

  private atualizarGridProdutos(): void {
    const pedidoId = this.pedidoForm.get('id')?.value;
    if (pedidoId) {
      this.pedidoProdutoService.listarPorPedido(pedidoId).subscribe({
        next: (produtos) => {
          console.log('Produtos:', produtos);
          this.produtos = produtos.map(p => ({
            id: p.id!,
            produtoId: p.produtoId.toString(),
            codigoEstoque: (p as any).codigoEstoque,
            descricao: p.descricao,
            quantidade: p.quantidade,
            preco: (p as any).precoVenda,
            desconto: p.descontoValor,
            valorFinal: p.valorFinalProduto
          }));
          this.atualizarTotais();
        }
      });
    }
  }

  private limparCamposProduto(): void {
    this.codigoEstoque = '';
    this.quantidade = 1;
    this.percentualDesconto = 0;
    this.valorDesconto = 0;
    this.valorUnitario = 0;
    this.descricaoProduto = '';
  }

  private carregarCondicoesPagamento(): void {
    this.carregandoCondicoesPagamento = true;
    
    this.condicaoPagamentoService.listar().subscribe({
      next: (condicoes) => {
        this.condicoesPagamento = condicoes;
        this.carregandoCondicoesPagamento = false;
      },
      error: (error) => {
        console.error('Erro ao carregar condições de pagamento:', error);
        this.carregandoCondicoesPagamento = false;
      }
    });
  }

  private carregarFormasPagamento(): void {
    this.carregandoFormasPagamento = true;
    
    
    this.formaPagamentoService.listar().subscribe({
      next: (formas) => {
        
        this.formasPagamento = formas;
        this.carregandoFormasPagamento = false;
      },
      error: (error) => {
        
        this.carregandoFormasPagamento = false;
      }
    });
  }

  private carregarStatusPedido(): void {
    this.carregandoStatus = true;
    this.pedidoStatusService.listar().subscribe({
      next: (status) => {
        this.statusPedido = status;
        this.carregandoStatus = false;
      },
      error: (error) => {
        console.error('Erro ao carregar status do pedido:', error);
        this.carregandoStatus = false;
      }
    });
  }

  public actionComplete(args: any): void {
    if (args.requestType === 'delete') {
      const pedidoProdutoId = args.data[0].id;
      if (pedidoProdutoId) {
        this.pedidoProdutoService.excluir(pedidoProdutoId).subscribe({
          next: () => {
            this.toastObj.show({
              title: 'Sucesso',
              content: 'Produto removido do pedido.',
              cssClass: 'e-success',
              timeOut: 3000
            });
            const index = this.produtos.findIndex(p => p.id === pedidoProdutoId);
            if (index > -1) {
              this.produtos.splice(index, 1);
              this.produtos = [...this.produtos];
            }
            this.atualizarTotais();
          },
          error: (error) => {
            this.toastObj.show({
              title: 'Erro',
              content: 'Erro ao remover produto. Tente novamente.',
              cssClass: 'e-error',
              timeOut: 3000
            });
            this.atualizarGridProdutos();
          }
        });
      }
    }
  }

  private atualizarTotais(): void {
    const subtotal = this.calcularSubtotal();
    const frete = this.pedidoForm.get('valorFrete')?.value || 0;
    const valorTotal = subtotal + frete;

    const somaDescontos = this.produtos.reduce((acc, produto) => acc + produto.desconto, 0);
    const denominador = somaDescontos + subtotal;
    const descontoGeral = (denominador > 0) ? (somaDescontos / denominador) : 0;

    this.pedidoForm.patchValue({
      valorTotal: valorTotal,
      descontoGeral: descontoGeral
    });
  }

  onSalvar(): void {
    console.log('Início onSalvar - salvando:', this.salvando, 'salvamentoEmAndamento:', this.salvamentoEmAndamento);
    
    // Evitar múltiplas operações de salvamento simultâneas
    if (this.salvamentoEmAndamento) {
      console.log('Operação de salvamento já em andamento, ignorando nova solicitação');
      return;
    }
    
    // Garantir que salvando seja false no início do método
    // Isso corrige qualquer estado inconsistente que possa ter ocorrido
    this.salvando = false;
    this.salvamentoEmAndamento = true;
    
    // Configurar um timeout de segurança para garantir que salvando seja redefinido
    // mesmo se ocorrer algum erro não tratado
    const resetSalvandoTimeout = setTimeout(() => {
      if (this.salvando || this.salvamentoEmAndamento) {
        console.log('Forçando salvando = false após timeout de segurança');
        this.salvando = false;
        this.salvamentoEmAndamento = false;
      }
    }, 5000);
    
    // Função para redefinir o estado de salvamento
    const resetSalvandoState = () => {
      this.salvando = false;
      this.salvamentoEmAndamento = false;
      console.log('Estado de salvamento redefinido - salvando:', this.salvando, 'salvamentoEmAndamento:', this.salvamentoEmAndamento);
    };
    
    try {
      if (!this.pedidoSalvo) {
        // Salvar dados básicos e gerar código
        if (this.pedidoForm.get('clienteId')?.valid && this.pedidoForm.get('statusPedidoId')?.valid) {
          console.log('Salvando dados básicos...');
          this.salvando = true;
          this.pedidoService.gerarNovoPedidoCodigo().subscribe({
            next: (codigo) => {
              console.log('Código gerado:', codigo);
              const codigoFormatado = `bcz${codigo.toString().padStart(5, '0')}`;
              this.pedidoForm.patchValue({ numeroPedido: codigo });
              const pedidoBasico = {
                pedidoCodigo: codigo,
                dataLancamento: new Date(),
                clienteId: this.pedidoForm.get('clienteId')?.value,
                pedidoStatusId: this.pedidoForm.get('statusPedidoId')?.value,
                dataAlteracao: new Date(),
                usuarioModificacaoId: 1 // TODO: pegar do auth
              };
              this.pedidoService.criar(pedidoBasico as any).subscribe({
                next: (pedidoSalvo) => {
                  console.log('Dados básicos salvos com sucesso');
                  this.toastObj.show({
                    title: 'Sucesso',
                    content: 'Dados básicos salvos! Continue preenchendo o pedido.',
                    cssClass: 'e-success',
                    timeOut: 3000
                  });
                  this.pedidoSalvo = true;
                  this.habilitarCamposDetalhes();
                  (this.pedidoForm.patchValue as any)({
                    id: pedidoSalvo.id,
                    dataLancamento: pedidoSalvo['dataLancamento'] || pedidoSalvo['DataLancamento']
                  });
                  resetSalvandoState();
                  console.log('Após salvar dados básicos - salvando:', this.salvando);
                },
                error: (error) => {
                  console.error('Erro ao salvar dados básicos:', error);
                  this.toastObj.show({
                    title: 'Erro',
                    content: 'Erro ao salvar dados básicos.',
                    cssClass: 'e-error',
                    timeOut: 3000
                  });
                  resetSalvandoState();
                  console.log('Após erro ao salvar dados básicos - salvando:', this.salvando);
                },
                complete: () => {
                  // Garantir que salvando seja false quando a operação for concluída
                  resetSalvandoState();
                }
              });
            },
            error: (error) => {
              console.error('Erro ao gerar código do pedido:', error);
              this.toastObj.show({
                title: 'Erro',
                content: 'Erro ao gerar código do pedido.',
                cssClass: 'e-error',
                timeOut: 3000
              });
              resetSalvandoState();
              console.log('Após erro ao gerar código - salvando:', this.salvando);
            },
            complete: () => {
              // Garantir que salvando seja false quando a operação for concluída
              // Não redefinimos aqui porque ainda podemos estar no meio da operação de criar
            }
          });
        } else {
          console.log('Formulário inválido - dados básicos');
          this.pedidoForm.get('clienteId')?.markAsTouched();
          this.pedidoForm.get('statusPedidoId')?.markAsTouched();
          this.toastObj.show({
            title: 'Atenção',
            content: 'Preencha Cliente e Status para iniciar o pedido.',
            cssClass: 'e-warning',
            timeOut: 3000
          });
          resetSalvandoState();
        }
      } else {
        // Salvar o restante do pedido normalmente
        if (this.pedidoForm.valid && this.produtos.length > 0) {
          console.log('Salvando pedido completo...');
          this.salvando = true;
          // Montar o payload conforme esperado pela API
          const payload: any = {
            id: this.pedidoForm.get('id')?.value,
            numeroPedido: this.pedidoForm.get('numeroPedido')?.value,
            dataPedido: new Date().toISOString(),
            pessoaId: this.pedidoForm.get('clienteId')?.value,
            valorFrete: this.pedidoForm.get('valorFrete')?.value || 0,
            statusPedidoId: this.pedidoForm.get('statusPedidoId')?.value,
            valorTotal: this.calcularValorTotal(),
            condicaoPagamentoId: this.pedidoForm.get('condicaoPagamento')?.value,
            formaPagamentoId: this.pedidoForm.get('formaPagamento')?.value,
            enderecoId: this.pedidoForm.get('enderecoId')?.value,
            observacao: this.pedidoForm.get('observacao')?.value,
            usuarioModificacaoId: 1 // TODO: Pegar do serviço de autenticação
          };
          const id = this.pedidoForm.get('id')?.value;
          if (!id) {
            console.error('ID do pedido não encontrado');
            this.toastObj.show({
              title: 'Erro',
              content: 'ID do pedido não encontrado. Salve os dados básicos antes de continuar.',
              cssClass: 'e-error',
              timeOut: 3000
            });
            resetSalvandoState();
            return;
          }
          this.pedidoService.atualizar(id, payload).subscribe({
            next: (pedidoSalvo) => {
              console.log('Pedido salvo com sucesso');
              this.toastObj.show({
                title: 'Sucesso',
                content: 'Pedido salvo com sucesso!',
                cssClass: 'e-success',
                timeOut: 3000
              });
              // Removida a navegação para permanecer na tela de cadastro
              (this.pedidoForm.patchValue as any)({
                id: pedidoSalvo.id,
                dataLancamento: pedidoSalvo['dataLancamento'] || pedidoSalvo['DataLancamento']
              });
              resetSalvandoState();
              console.log('Após salvar pedido completo - salvando:', this.salvando);
            },
            error: (error) => {
              console.error('Erro ao salvar pedido:', error);
              this.toastObj.show({
                title: 'Erro',
                content: 'Erro ao salvar pedido. Tente novamente.',
                cssClass: 'e-error',
                timeOut: 3000
              });
              resetSalvandoState();
              console.log('Após erro ao salvar pedido completo - salvando:', this.salvando);
            },
            complete: () => {
              // Garantir que salvando seja false quando a operação for concluída
              resetSalvandoState();
            }
          });
        } else {
          console.log('Formulário inválido ou sem produtos');
          this.pedidoForm.markAllAsTouched();
          if (this.produtos.length === 0) {
            this.toastObj.show({
              title: 'Atenção',
              content: 'Adicione pelo menos um produto ao pedido.',
              cssClass: 'e-warning',
              timeOut: 3000
            });
          }
          resetSalvandoState();
        }
      }
    } catch (error) {
      // Capturar qualquer erro não tratado e garantir que salvando seja false
      console.error('Erro não tratado no método onSalvar:', error);
      this.toastObj.show({
        title: 'Erro',
        content: 'Ocorreu um erro inesperado. Tente novamente.',
        cssClass: 'e-error',
        timeOut: 3000
      });
      resetSalvandoState();
    } finally {
      // Garantir que salvando seja false no final do método
      // Isso é uma medida de segurança adicional
      setTimeout(() => {
        if (this.salvando || this.salvamentoEmAndamento) {
          console.log('Forçando salvando = false após timeout');
          resetSalvandoState();
        }
        // Limpar o timeout de segurança
        clearTimeout(resetSalvandoTimeout);
      }, 100);
    }
  }

  private calcularValorTotal(): number {
    const subtotal = this.calcularSubtotal();
    const frete = this.pedidoForm.get('valorFrete')?.value || 0;
    return subtotal + frete;
  }

  private calcularSubtotal(): number {
    return this.produtos.reduce((total, produto) => total + produto.valorFinal, 0);
  }

  onVoltar(): void {
    this.router.navigate(['/pedidos']);
  }

  onNovoPedido(): void {
    this.router.navigate(['/pedidos/novo']);
  }

  onGerarNFe(): void {
    console.log('Gerando NFe...');
    // TODO: Implementar chamada à API
  }

  onGerarNFCe(): void {
    console.log('Gerando NFCe...');
    // TODO: Implementar chamada à API
  }

  onImprimir(): void {
    console.log('Imprimindo pedido...');
    // TODO: Implementar chamada à API
  }

  private carregarPedido(id: number): void {
    this.pedidoService.obterPorId(id).subscribe({
      next: (pedido: any) => {
        this.habilitarCamposDetalhes();
        console.log('Dados do pedido recebidos da API:', pedido);

        // Processar a data de lançamento
        let dataLancamento: Date;
        if (pedido.dataLancamento) {
          dataLancamento = new Date(pedido.dataLancamento);
        } else if (pedido.DataLancamento) {
          dataLancamento = new Date(pedido.DataLancamento);
        } else {
          dataLancamento = new Date();
        }

        // Extrair a hora do lançamento
        const horaLancamento = dataLancamento.getHours().toString().padStart(2, '0') + ':' + 
                              dataLancamento.getMinutes().toString().padStart(2, '0');

        this.pedidoForm.patchValue({
          id: pedido.id,
          statusPedidoId: pedido.statusPedidoId || pedido.pedidoStatusId || pedido.pedidoStatusID,
          clienteId: pedido.pessoaId || pedido.clienteId || pedido.clienteID,
          pessoaId: pedido.pessoaId || pedido.clienteId || pedido.clienteID,
          enderecoId: pedido.enderecoId || pedido.enderecoEntregaId || pedido.enderecoEntregaID,
          valorFrete: pedido.valorFrete || 0,
          descontoGeral: pedido.descontoGeral || pedido.descontoPorcentagem || 0,
          condicaoPagamento: pedido.condicaoPagamentoId || pedido.condicaoPagamentoID,
          formaPagamento: pedido.formaPagamentoId || pedido.formaPagamentoID,
          observacao: pedido.observacao || pedido.observacoes,
          dataLancamento: dataLancamento,
          horaLancamento: horaLancamento
        });

        // Verificar e definir o número do pedido
        if (pedido.numeroPedido) {
          this.pedidoForm.get('numeroPedido')?.setValue(pedido.numeroPedido);
        } else if (pedido.pedidoCodigo) {
          this.pedidoForm.get('numeroPedido')?.setValue(pedido.pedidoCodigo);
        }
        
        // Verificar e definir o valor total
        if (pedido.valorTotal !== undefined && pedido.valorTotal !== null) {
          this.pedidoForm.get('valorTotal')?.setValue(pedido.valorTotal);
        } else {
          // Se não houver valor total, calcular com base nos produtos
          this.atualizarTotais();
        }

        this.pedidoSalvo = true;
        this.atualizarGridProdutos();
      },
      error: (err) => {
        console.error('Erro ao carregar pedido:', err);
        this.toastObj.show({
          title: 'Erro',
          content: 'Erro ao carregar pedido.',
          cssClass: 'e-error',
          timeOut: 3000
        });
      }
    });
  }

  onCodigoEstoqueChange(event: any): void {
    const codigoEstoque = event.value;
    if (codigoEstoque && codigoEstoque.length > 0) {
      this.carregandoProduto = true;
      this.estoqueService.buscarPorCodigo(codigoEstoque).subscribe({
        next: (produto) => {
          if (produto) {
            this.codigoProduto = produto.produtoId;
            this.descricaoProduto = produto.descricao;
            this.valorUnitario = produto.precoVenda || 0;
          } else {
            this.toastObj.show({
              title: 'Atenção',
              content: 'Produto não encontrado',
              cssClass: 'e-warning',
              timeOut: 3000
            });
            this.descricaoProduto = '';
            this.valorUnitario = 0;
          }
          this.carregandoProduto = false;
        },
        error: (error) => {
          this.toastObj.show({
            title: 'Erro',
            content: 'Produto não encontrado',
            cssClass: 'e-error',
            timeOut: 3000
          });
          this.descricaoProduto = '';
          this.valorUnitario = 0;
          this.carregandoProduto = false;
        }
      });
    } else {
      this.descricaoProduto = '';
      this.valorUnitario = 0;
    }
  }

  private habilitarCamposDetalhes(): void {
    this.pedidoForm.get('enderecoId')?.enable();
    this.pedidoForm.get('descontoGeral')?.enable();
    this.pedidoForm.get('valorFrete')?.enable();
    this.pedidoForm.get('condicaoPagamento')?.enable();
    this.pedidoForm.get('formaPagamento')?.enable();
    this.pedidoForm.get('observacao')?.enable();
    // valorTotal permanece readonly
  }
  }

  
