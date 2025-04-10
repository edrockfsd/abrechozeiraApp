import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { GridComponent, GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule, DropDownListComponent } from '@syncfusion/ej2-angular-dropdowns';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { PedidoService } from '../../services/pedido.service';
import { EnderecoService } from '../../../pessoas/services/endereco.service';
import { PessoaService } from '../../../pessoas/services/pessoa.service';
import { Pedido, ItemPedido } from '../../interfaces/pedido.interface';
import { Pessoa } from '../../../pessoas/models/pessoa';

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
    ToastModule
  ]
})
export class CadastroPedidoComponent implements OnInit {
  @ViewChild('toast') public toast!: ToastComponent;
  @ViewChild('gridEnderecos') public gridEnderecos!: GridComponent;
  @ViewChild('gridProdutos') public gridProdutos!: GridComponent;
  @ViewChild('clienteDropdown') public clienteDropdown!: DropDownListComponent;

  public pedidoForm!: FormGroup;
  public enderecos: any[] = [];
  public enderecoSelecionado?: number;
  public isEditMode = false;
  public pedidoId?: number;
  public salvando = false;
  public carregandoEnderecos = false;
  public carregandoPessoa = false;
  public pessoas: Pessoa[] = [];
  public carregandoPessoas = false;
  public clientes: Pessoa[] = [];
  public clienteSelecionado?: Pessoa;
  
  public statusPedido = [
    { id: 1, descricao: 'Novo' },
    { id: 2, descricao: 'Em Processamento' },
    { id: 3, descricao: 'Aguardando Pagamento' },
    { id: 4, descricao: 'Pago' },
    { id: 5, descricao: 'Enviado' },
    { id: 6, descricao: 'Entregue' },
    { id: 7, descricao: 'Cancelado' }
  ];

  public formasPagamento = [
    { id: 1, descricao: 'Transferência Bancária' },
    { id: 2, descricao: 'PIX' },
    { id: 3, descricao: 'Cartão de Crédito' },
    { id: 4, descricao: 'Cartão de Débito' },
    { id: 5, descricao: 'Dinheiro' }
  ];

  public itensPedido: ItemPedido[] = [];
  public sequencialItem = 1;

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
    private pedidoService: PedidoService,
    private enderecoService: EnderecoService,
    private pessoaService: PessoaService
  ) {}

  ngOnInit(): void {
    this.initForm();
    
    this.route.params.subscribe(params => {
      if (params['pessoaId']) {
        this.carregarPessoa(+params['pessoaId']);
        this.carregarEnderecos(+params['pessoaId']);
      }
      
      if (params['id']) {
        this.pedidoId = +params['id'];
        this.isEditMode = true;
        this.carregarPedido();
      } else {
        this.gerarNumeroPedido();
      }
    });

    this.carregarPessoas();
  }

  private initForm(): void {
    this.pedidoForm = this.formBuilder.group({
      numeroPedido: ['', Validators.required],
      dataPedido: [new Date(), Validators.required],
      statusPedidoId: [1, Validators.required],
      pessoaId: [null, Validators.required],
      nomeCompleto: ['', Validators.required],
      cpf: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      telefone: ['', Validators.required],
      enderecoId: [null, Validators.required],
      observacao: [''],
      formaPagamento: ['', Validators.required],
      parcelas: [1, Validators.required],
      valorSubtotal: [0],
      valorFrete: [0],
      valorTotal: [0]
    });
  }

  private gerarNumeroPedido(): void {
    const hoje = new Date();
    const ano = hoje.getFullYear();
    const mes = String(hoje.getMonth() + 1).padStart(2, '0');
    const dia = String(hoje.getDate()).padStart(2, '0');
    const sequencial = '001'; // Você pode implementar uma lógica mais robusta para gerar o sequencial
    
    this.pedidoForm.patchValue({
      numeroPedido: `PED-${ano}${mes}${dia}-${sequencial}`
    });
  }

  private carregarPessoa(pessoaId: number): void {
    this.carregandoPessoa = true;
    this.pessoaService.buscarPorId(pessoaId).subscribe({
      next: (pessoa) => {
        this.clienteSelecionado = pessoa;
        this.pedidoForm.patchValue({
          pessoaId: pessoa.id,
          nomeCompleto: pessoa.nome,
          cpf: pessoa.cpf,
          email: pessoa.email,
          telefone: pessoa.telefone
        });
        this.carregandoPessoa = false;
        this.carregarEnderecos(pessoa.id);
      },
      error: (error) => {
        console.error('Erro ao carregar pessoa:', error);
        this.toast.show({
          title: 'Erro',
          content: 'Erro ao carregar dados do cliente. Tente novamente.',
          cssClass: 'e-toast-danger',
          icon: 'e-error toast-icons'
        });
        this.carregandoPessoa = false;
      }
    });
  }

  private carregarEnderecos(pessoaId: number): void {
    this.carregandoEnderecos = true;
    this.enderecoService.listarPorPessoa(pessoaId).subscribe({
      next: (enderecos) => {
        this.enderecos = enderecos;
        this.carregandoEnderecos = false;
      },
      error: (error) => {
        console.error('Erro ao carregar endereços:', error);
        this.toast.show({
          title: 'Erro',
          content: 'Erro ao carregar endereços. Tente novamente.',
          cssClass: 'e-toast-danger',
          icon: 'e-error toast-icons'
        });
        this.carregandoEnderecos = false;
      }
    });
  }

  private carregarPedido(): void {
    if (this.pedidoId) {
      this.pedidoService.obterPorId(this.pedidoId).subscribe({
        next: (pedido) => {
          this.pedidoForm.patchValue({
            numeroPedido: pedido.numeroPedido,
            dataPedido: new Date(pedido.dataPedido),
            statusPedidoId: pedido.statusPedidoId,
            observacao: pedido.observacao,
            formaPagamento: pedido.formaPagamento,
            parcelas: pedido.parcelas,
            valorSubtotal: pedido.valorSubtotal,
            valorFrete: pedido.valorFrete,
            valorTotal: pedido.valorTotal
          });
          this.enderecoSelecionado = pedido.enderecoId;
          this.itensPedido = pedido.itens;
          this.sequencialItem = Math.max(...pedido.itens.map(item => item.sequencial)) + 1;
          
          // Carregar dados do cliente
          this.carregarPessoa(pedido.pessoaId);
        },
        error: (error) => {
          console.error('Erro ao carregar pedido:', error);
          this.toast.show({
            title: 'Erro',
            content: 'Erro ao carregar pedido. Tente novamente.',
            cssClass: 'e-toast-danger',
            icon: 'e-error toast-icons'
          });
        }
      });
    }
  }

  onClienteChange(args: any): void {
    if (args.value) {
      this.carregarPessoa(args.value);
    } else {
      this.limparDadosCliente();
    }
  }

  private limparDadosCliente(): void {
    this.clienteSelecionado = undefined;
    this.enderecos = [];
    this.enderecoSelecionado = undefined;
    this.pedidoForm.patchValue({
      pessoaId: null,
      nomeCompleto: '',
      cpf: '',
      email: '',
      telefone: '',
      enderecoId: null
    });
  }

  onEnderecoChange(args: any): void {
    if (args.target) {
      this.enderecoSelecionado = args.target.value;
      this.pedidoForm.patchValue({
        enderecoId: this.enderecoSelecionado
      });
    }
  }

  onAdicionarProduto(): void {
    const novoItem: ItemPedido = {
      sequencial: this.sequencialItem++,
      produtoId: 0,
      produto: '',
      condicao: '',
      categoria: '',
      tamanho: '',
      cor: '',
      quantidade: 1,
      valorUnitario: 0,
      valorSubtotal: 0
    };
    
    this.itensPedido = [...this.itensPedido, novoItem];
    this.calcularTotais();
  }

  onRemoverProduto(item: ItemPedido): void {
    this.itensPedido = this.itensPedido.filter(i => i.sequencial !== item.sequencial);
    this.calcularTotais();
  }

  onQuantidadeChange(item: ItemPedido): void {
    item.valorSubtotal = item.quantidade * item.valorUnitario;
    this.calcularTotais();
  }

  onValorUnitarioChange(item: ItemPedido): void {
    item.valorSubtotal = item.quantidade * item.valorUnitario;
    this.calcularTotais();
  }

  private calcularTotais(): void {
    const valorSubtotal = this.itensPedido.reduce((total, item) => total + item.valorSubtotal, 0);
    const valorFrete = this.pedidoForm.get('valorFrete')?.value || 0;
    
    this.pedidoForm.patchValue({
      valorSubtotal: valorSubtotal,
      valorTotal: valorSubtotal + valorFrete
    });
  }

  onFreteChange(): void {
    this.calcularTotais();
  }

  onSubmit(): void {
    if (this.pedidoForm.valid && !this.salvando && this.enderecoSelecionado && this.itensPedido.length > 0) {
      this.salvando = true;
      
      const pedido: Pedido = {
        ...this.pedidoForm.value,
        enderecoId: this.enderecoSelecionado,
        itens: this.itensPedido
      };

      const operacao = this.isEditMode
        ? this.pedidoService.atualizar(this.pedidoId!, pedido)
        : this.pedidoService.criar(pedido);

      operacao.subscribe({
        next: () => {
          this.toast.show({
            title: 'Sucesso',
            content: `Pedido ${this.isEditMode ? 'atualizado' : 'cadastrado'} com sucesso!`,
            cssClass: 'e-toast-success',
            icon: 'e-success toast-icons'
          });
          this.router.navigate(['/pedidos']);
        },
        error: (error) => {
          console.error('Erro ao salvar pedido:', error);
          this.toast.show({
            title: 'Erro',
            content: 'Erro ao salvar pedido. Tente novamente.',
            cssClass: 'e-toast-danger',
            icon: 'e-error toast-icons'
          });
          this.salvando = false;
        }
      });
    } else {
      this.pedidoForm.markAllAsTouched();
      if (!this.enderecoSelecionado) {
        this.toast.show({
          title: 'Atenção',
          content: 'Por favor, selecione um endereço de entrega.',
          cssClass: 'e-toast-warning',
          icon: 'e-warning toast-icons'
        });
      } else if (this.itensPedido.length === 0) {
        this.toast.show({
          title: 'Atenção',
          content: 'Por favor, adicione pelo menos um produto ao pedido.',
          cssClass: 'e-toast-warning',
          icon: 'e-warning toast-icons'
        });
      } else {
        this.toast.show({
          title: 'Atenção',
          content: 'Por favor, preencha todos os campos obrigatórios.',
          cssClass: 'e-toast-warning',
          icon: 'e-warning toast-icons'
        });
      }
    }
  }

  onSalvarRascunho(): void {
    // Implementar lógica para salvar como rascunho
    this.pedidoForm.patchValue({ statusPedidoId: 1 }); // Status "Novo"
    this.onSubmit();
  }

  onFinalizarPedido(): void {
    // Implementar lógica para finalizar pedido
    this.pedidoForm.patchValue({ statusPedidoId: 2 }); // Status "Em Processamento"
    this.onSubmit();
  }

  onCancelar(): void {
    this.router.navigate(['/pedidos']);
  }

  onVoltar(): void {
    this.router.navigate(['/pedidos']);
  }

  carregarPessoas(): void {
    this.carregandoPessoas = true;
    this.pessoaService.listar().subscribe({
      next: (pessoas) => {
        this.clientes = pessoas;
        this.carregandoPessoas = false;
      },
      error: (error) => {
        console.error('Erro ao carregar pessoas:', error);
        this.carregandoPessoas = false;
      }
    });
  }
} 