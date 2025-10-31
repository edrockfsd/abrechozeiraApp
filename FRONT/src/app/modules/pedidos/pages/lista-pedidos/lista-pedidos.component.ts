import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { L10n, loadCldr, setCulture, setCurrencyCode } from '@syncfusion/ej2-base';
import { GridComponent, GridModule, PageService, SortService, FilterService, ToolbarService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { PedidoService } from '../../services/pedido.service';


@Component({
  selector: 'app-lista-pedidos',
  templateUrl: './lista-pedidos.component.html',
  styleUrls: ['./lista-pedidos.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    ButtonModule,
    ToastModule
  ],
  providers: [
    PageService,
    SortService,
    FilterService,
    ToolbarService
  ]
})
export class ListaPedidosComponent implements OnInit {
  @ViewChild('grid') public grid!: GridComponent;
  @ViewChild('toast') public toast!: ToastComponent;

  public data: any[] = [];
  public carregando = false;

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  public gridSettings = {
    columns: [
      { field: 'id', headerText: 'ID', width: 100, textAlign: 'Right', visible: false },
      { field: 'numeroPedido', headerText: 'Código', width: 140 },
      { field: 'clienteNome', headerText: 'Cliente', width: 200 },
      { field: 'clienteNick', headerText: 'NickName', width: 120 },
      { field: 'cep', headerText: 'CEP', width: 110 },
      { field: 'endereco', headerText: 'Endereço', width: 300 },
      { field: 'dataPedidoTexto', headerText: 'Data do Pedido', width: 150 },
      { field: 'valorFrete', headerText: 'Valor Frete', width: 150, format: 'C2' },
      { field: 'valorTotal', headerText: 'Valor Total', width: 150, format: 'C2' },
      { field: 'status', headerText: 'Status', width: 150 }
    ],
    pageSettings: { pageSize: 10 },
    toolbar: ['Search'],
    searchSettings: { fields: ['numeroPedido', 'clienteNome', 'clienteNick', 'cep', 'endereco', 'dataPedidoTexto', 'valorFrete', 'valorTotal', 'status'], operator: 'contains', key: '', ignoreCase: true }
  };

  constructor(
    private router: Router,
    private pedidoService: PedidoService
  ) {}

  ngOnInit(): void {
    this.carregarPedidos();
  }

  private carregarPedidos(): void {
    this.carregando = true;
    this.pedidoService.listar().subscribe({
      next: (pedidos) => {
        this.data = (pedidos as any[]).map((p: any) => {
          const dataRaw = p.dataPedido ?? p.DataPedido;
          const dataObj = dataRaw ? new Date(dataRaw) : undefined;
          const dataTexto = dataObj ? this.formatarDataDDMMYYYY(dataObj) : '';
          return {
            ...p,
            // Garantir campos em camelCase usados no grid
            clienteNick: p.clienteNick ?? p.ClienteNick,
            clienteNome: p.clienteNome ?? p.ClienteNome,
            cep: p.cep ?? p.Cep ?? p.CEP ?? '',
            dataPedido: dataObj,
            dataPedidoTexto: dataTexto,
            endereco: p.endereco ?? p.Endereco,
            // Formatar código do pedido como na tela de edição
            numeroPedido: this.formatarPedidoCodigo(p.pedidoCodigo ?? p.PedidoCodigo)
          };
        });
        this.carregando = false;
      },
      error: (error) => {
        console.error('Erro ao carregar pedidos:', error);
        this.toast.show({
          title: 'Erro',
          content: 'Erro ao carregar pedidos. Tente novamente.',
          cssClass: 'e-toast-danger',
          icon: 'e-error toast-icons'
        });
        this.carregando = false;
      }
    });
  }

  private formatarPedidoCodigo(codigo: number | string): string {
    if (codigo === null || codigo === undefined) return '';
    const codigoStr = String(codigo).replace(/^bcz/i, '');
    return `bcz${codigoStr.padStart(7, '0')}`;
  }

  private formatarDataDDMMYYYY(data: Date): string {
    const dd = String(data.getDate()).padStart(2, '0');
    const mm = String(data.getMonth() + 1).padStart(2, '0');
    const yyyyISOWeek = data.getFullYear();
    // O cliente solicitou exatamente dd/MM/YYYY (YYYY maiúsculo)
    // Retornamos nesse formato literal conforme pedido
    return `${dd}/${mm}/${yyyyISOWeek}`;
  }

  onNovoPedido(): void {
    this.router.navigate(['/pedidos/novo']);
  }

  onEditar(pedido: any): void {
    this.router.navigate(['/pedidos/editar', pedido.id]);
  }
} 
