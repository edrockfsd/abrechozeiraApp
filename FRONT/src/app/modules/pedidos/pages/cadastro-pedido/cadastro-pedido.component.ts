import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ToastModule } from '@syncfusion/ej2-angular-notifications';
import { NumericTextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { PessoaService } from '../../../pessoas/services/pessoa.service';
import { EnderecoService } from '../../../pessoas/services/endereco.service';
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
  public quantidade: number = 1;
  public percentualDesconto: number = 0;
  public valorDesconto: number = 0;
  public valorUnitario: number = 0;
  public produtos: Produto[] = [];
  public condicoesPagamento: any[] = [];
  public formasPagamento: any[] = [];
  public enderecos: Endereco[] = [];

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private pessoaService: PessoaService,
    private enderecoService: EnderecoService
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
    // TODO: Implementar chamada à API quando estiver disponível
    this.condicoesPagamento = [
      { id: 1, descricao: 'À Vista' },
      { id: 2, descricao: '30 Dias' },
      { id: 3, descricao: '30/60 Dias' }
    ];
  }

  private carregarFormasPagamento(): void {
    // TODO: Implementar chamada à API quando estiver disponível
    this.formasPagamento = [
      { id: 1, descricao: 'Dinheiro' },
      { id: 2, descricao: 'Cartão de Crédito' },
      { id: 3, descricao: 'Cartão de Débito' },
      { id: 4, descricao: 'PIX' }
    ];
  }
} 