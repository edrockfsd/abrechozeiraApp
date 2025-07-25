import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
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
      { field: 'id', headerText: 'ID', width: 100, textAlign: 'Right' },
      { field: 'cliente', headerText: 'Cliente', width: 200 },
      { field: 'dataPedido', headerText: 'Data do Pedido', width: 150, format: 'dd/MM/yyyy' },
      { field: 'valorTotal', headerText: 'Valor Total', width: 150, format: 'C2' },
      { field: 'status', headerText: 'Status', width: 150 }
    ],
    pageSettings: { pageSize: 10 },
    toolbar: ['Search'],
    searchSettings: { fields: ['cliente', 'status'], operator: 'contains', key: '', ignoreCase: true }
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
        this.data = pedidos;
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

  onNovoPedido(): void {
    this.router.navigate(['/pedidos/novo']);
  }

  onEditar(pedido: any): void {
    this.router.navigate(['/pedidos/editar', pedido.id]);
  }
} 