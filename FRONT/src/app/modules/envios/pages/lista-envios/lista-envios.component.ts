import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { GridComponent, GridModule, PageService, SortService, FilterService, ToolbarService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { SuperfreteService } from '../../services/superfrete.service';
import { EtiquetaInfo, SUPERFRETE_SERVICOS } from '../../interfaces/superfrete.interface';

@Component({
  selector: 'app-lista-envios',
  templateUrl: './lista-envios.component.html',
  styleUrls: ['./lista-envios.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    ButtonModule,
    ToastModule,
    FormsModule
  ],
  providers: [PageService, SortService, FilterService, ToolbarService]
})
export class ListaEnviosComponent implements OnInit, AfterViewInit {
  @ViewChild('grid') public grid!: GridComponent;
  @ViewChild('toast') public toast!: ToastComponent;

  public data: EtiquetaInfo[] = [];
  public rawData: EtiquetaInfo[] = [];
  public carregando = false;

  public filtroStatus = 'all';
  public filtroDataInicio = '';
  public filtroDataFim = '';

  public filterSettings = {
    type: 'FilterBar'
  };

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  public gridSettings = {
    columns: [
      { field: 'id',           headerText: 'ID Etiqueta',    width: 220 },
      { field: 'protocol',     headerText: 'Protocolo',      width: 140 },
      { field: 'destinatario', headerText: 'Destinatário',   width: 200 },
      { field: 'service_name', headerText: 'Serviço',        width: 140 },
      { field: 'tracking',     headerText: 'Rastreio',       width: 180 },
      { field: 'price',        headerText: 'Valor',          width: 120, format: 'C2' },
      { field: 'statusLabel',  headerText: 'Status',         width: 180 },
      { field: 'created_at',   headerText: 'Data Criação',   width: 160 },
    ],
    pageSettings: { pageSize: 15 },
    toolbar: ['Search'],
    searchSettings: {
      fields: ['id', 'protocol', 'destinatario', 'service_name', 'tracking', 'statusLabel'],
      operator: 'contains',
      key: '',
      ignoreCase: true
    }
  };

  constructor(
    private superfreteService: SuperfreteService,
    private router: Router
  ) {}

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    this.carregarEtiquetas();
  }

  carregarEtiquetas(): void {
    this.carregando = true;
    console.log('Iniciando requisição de carregar etiquetas do Superfrete...');
    this.superfreteService.listarEtiquetas().subscribe({
      next: (etiquetas) => {
        console.log('API Superfrete retornou com sucesso. Dados brutos:', etiquetas);
        this.rawData = (etiquetas || [])
          .filter(e => !!e)
          .map(e => ({
            ...e,
            created_at_raw: e.created_at,
            created_at: e.created_at ? this.formatarData(e.created_at) : '',
            tracking: e.tracking || 'Aguardando postagem'
          }));
        console.log('Dados mapeados e armazenados em rawData:', this.rawData);
        this.filtrarDados();
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao carregar etiquetas do Superfrete:', err);
        if (this.toast) {
          this.toast.show({
            title: 'Erro',
            content: 'Não foi possível carregar as etiquetas do Superfrete.',
            cssClass: 'e-toast-danger',
            icon: 'e-error toast-icons'
          });
        } else {
          console.warn('Toast component not ready to display error block.');
        }
        this.carregando = false;
      }
    });
  }

  filtrarDados(): void {
    console.log('Executando filtrarDados(). Filtros ativos:', {
      status: this.filtroStatus,
      dataInicio: this.filtroDataInicio,
      dataFim: this.filtroDataFim
    });
    
    let temp = [...this.rawData];

    // 1. Filtrar por Status
    if (this.filtroStatus && this.filtroStatus !== 'all') {
      temp = temp.filter(e => e.status === this.filtroStatus);
    }

    // 2. Filtrar por Data Inicial (comparando o início do dia)
    if (this.filtroDataInicio) {
      const dataInicio = new Date(this.filtroDataInicio + 'T00:00:00');
      temp = temp.filter(e => {
        if (!e.created_at_raw) return false;
        const dataCriacao = new Date(e.created_at_raw);
        return dataCriacao >= dataInicio;
      });
    }

    // 3. Filtrar por Data Final (comparando o fim do dia)
    if (this.filtroDataFim) {
      const dataFim = new Date(this.filtroDataFim + 'T23:59:59');
      temp = temp.filter(e => {
        if (!e.created_at_raw) return false;
        const dataCriacao = new Date(e.created_at_raw);
        return dataCriacao <= dataFim;
      });
    }

    this.data = temp;
    console.log('Resultado final do filtro em this.data:', this.data);
  }

  limparFiltros(): void {
    this.filtroStatus = 'all';
    this.filtroDataInicio = '';
    this.filtroDataFim = '';
    this.filtrarDados();
  }

  onNovoPedido(): void {
    this.router.navigate(['/pedidos/novo']);
  }

  onImportarWhatsApp(): void {
    this.router.navigate(['/envios/importar']);
  }

  private formatarData(dateStr: string): string {
    try {
      const d = new Date(dateStr);
      return `${String(d.getDate()).padStart(2,'0')}/${String(d.getMonth()+1).padStart(2,'0')}/${d.getFullYear()} ${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}`;
    } catch {
      return dateStr;
    }
  }

  getStatusClass(status: string): string {
    const map: Record<string,string> = {
      pending: 'badge-pending',
      released: 'badge-released',
      posted: 'badge-posted',
      delivered: 'badge-delivered',
      cancelled: 'badge-cancelled',
    };
    return map[status] ?? '';
  }

  nomeServico(id: number): string {
    return SUPERFRETE_SERVICOS[id] ?? `Serviço ${id}`;
  }
}
