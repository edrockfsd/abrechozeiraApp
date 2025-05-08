import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ToastModule } from '@syncfusion/ej2-angular-notifications';
import { NumericTextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { PessoaService } from '../../../pessoas/services/pessoa.service';
import { EnderecoService } from '../../../pessoas/services/endereco.service';
import { CondicaoPagamentoService, CondicaoPagamento } from '../../services/condicao-pagamento.service';
import { FormaPagamentoService, FormaPagamento } from '../../services/forma-pagamento.service';
import { EstoqueService, ProdutoEstoque } from '../../services/estoque.service';

interface Produto {
  id: number;
  codigo: string;
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
    NumericTextBoxModule
  ]
})
export class CadastroPedidoComponent implements OnInit {
  public pedidoForm!: FormGroup;
  public clientes: any[] = [];
  public carregandoClientes = false;
  public codigoProduto: string = '';
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

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private pessoaService: PessoaService,
    private enderecoService: EnderecoService,
    private condicaoPagamentoService: CondicaoPagamentoService,
    private formaPagamentoService: FormaPagamentoService,
    private estoqueService: EstoqueService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.carregarClientes();
    this.carregarCondicoesPagamento();
    this.carregarFormasPagamento();
  }

  private initForm(): void {
    this.pedidoForm = this.formBuilder.group({
      id: [null],
      numeroPedido: [{ value: '', disabled: true }],
      dataLancamento: [{ value: new Date(), disabled: true }],
      horaLancamento: [{ value: this.getHoraAtual(), disabled: true }],
      clienteId: [null, Validators.required],
      pessoaId: [null, Validators.required],
      enderecoId: [null, Validators.required],
      descontoGeral: [0],
      valorFrete: [0],
      valorTotal: [{ value: 0, disabled: true }],
      condicaoPagamento: [null, Validators.required],
      formaPagamento: [null, Validators.required],
      observacao: [''],
      usuarioCriacaoId: [null],
      usuarioModificacaoId: [null]
    });

    this.gerarNumeroPedido();

    // Observa mudanças no clienteId para carregar os endereços
    this.pedidoForm.get('clienteId')?.valueChanges.subscribe(clienteId => {
      if (clienteId) {
        this.carregarEnderecos(clienteId);
      } else {
        this.enderecos = [];
      }
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

  onEnderecoSelecionado(event: any): void {
    if (event.data) {
      this.pedidoForm.patchValue({
        enderecoId: event.data.id
      });
    }
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

  private gerarNumeroPedido(): void {
    const hoje = new Date();
    const ano = hoje.getFullYear();
    const mes = String(hoje.getMonth() + 1).padStart(2, '0');
    const dia = String(hoje.getDate()).padStart(2, '0');
    const sequencial = '001';
    
    this.pedidoForm.patchValue({
      numeroPedido: `PED-${ano}${mes}${dia}-${sequencial}`
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
    if (!this.codigoProduto) {
      console.log('Por favor, informe o código do produto');
      return;
    }

    const valorFinal = (this.valorUnitario * this.quantidade) - this.valorDesconto;

    const novoProduto: Produto = {
      id: this.produtos.length + 1,
      codigo: this.codigoProduto,
      descricao: '', // Será preenchido quando a API estiver pronta
      quantidade: this.quantidade,
      preco: this.valorUnitario,
      desconto: this.valorDesconto,
      valorFinal: valorFinal
    };

    this.produtos.push(novoProduto);
    
    // Limpar campos
    this.codigoProduto = '';
    this.quantidade = 1;
    this.percentualDesconto = 0;
    this.valorDesconto = 0;
    this.valorUnitario = 0;
  }

  private carregarCondicoesPagamento(): void {
    this.carregandoCondicoesPagamento = true;
    console.log('Iniciando carregamento de condições de pagamento...');
    this.condicaoPagamentoService.listar().subscribe({
      next: (condicoes) => {
        console.log('Condições de pagamento carregadas:', condicoes);
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
    console.log('Iniciando carregamento de formas de pagamento...');
    this.formaPagamentoService.listar().subscribe({
      next: (formas) => {
        console.log('Formas de pagamento carregadas:', formas);
        this.formasPagamento = formas;
        this.carregandoFormasPagamento = false;
      },
      error: (error) => {
        console.error('Erro ao carregar formas de pagamento:', error);
        this.carregandoFormasPagamento = false;
      }
    });
  }

  onSalvar(): void {
    if (this.pedidoForm.valid) {
      console.log('Salvando pedido:', this.pedidoForm.value);
      // TODO: Implementar chamada à API
    } else {
      this.pedidoForm.markAllAsTouched();
    }
  }

  onCancelar(): void {
    this.router.navigate(['../'], { relativeTo: this.route });
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

  onCodigoProdutoChange(event: any): void {
    const codigo = event.value;
    if (codigo && codigo.length > 0) {
      this.carregandoProduto = true;
      this.estoqueService.buscarPorCodigo(codigo).subscribe({
        next: (produto) => {
          console.log('Produto encontrado:', produto);
          this.descricaoProduto = produto.descricao;
          this.valorUnitario = produto.precoVenda;
          this.carregandoProduto = false;
        },
        error: (error) => {
          console.error('Erro ao buscar produto:', error);
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
} 