import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { GridComponent, GridModule, PageService, SortService, FilterService, ToolbarService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { SuperfreteService } from '../../services/superfrete.service';
import { EnvioLoteService } from '../../services/envio-lote.service';
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
    private envioLoteService: EnvioLoteService,
    private router: Router
  ) {}

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    this.carregarEtiquetasEMapeamentos();
  }

  carregarEtiquetasEMapeamentos(): void {
    this.carregando = true;
    
    import('rxjs').then(rxjs => {
      rxjs.forkJoin({
        etiquetas: this.superfreteService.listarEtiquetas(),
        mapeamentos: this.envioLoteService.getMapeamentos()
      }).subscribe({
        next: (result: any) => {
          const etiquetas = result.etiquetas || [];
          const mapeamentosDict = result.mapeamentos || {};
          
          this.rawData = etiquetas
            .filter((e: any) => !!e)
            .map((e: any) => {
              // O C# pode serializar com PascalCase (EtiquetaId) ou camelCase (etiquetaId)
              const transacaoId = Object.keys(mapeamentosDict).find(k => {
                 const m = mapeamentosDict[k];
                 return m.etiquetaId === e.id || m.EtiquetaId === e.id;
              });
              const mapInfo = transacaoId ? mapeamentosDict[transacaoId] : null;

              return {
                ...e,
                created_at_raw: e.created_at,
                created_at: e.created_at ? this.formatarData(e.created_at) : '',
                tracking: e.tracking || 'Aguardando postagem',
                statusPagamento: mapInfo ? (mapInfo.statusPagamento || mapInfo.StatusPagamento) : 'N/A',
                statusSuperfrete: mapInfo ? (mapInfo.statusSuperfrete || mapInfo.StatusSuperfrete) : e.statusLabel,
                email: mapInfo ? (mapInfo.email || mapInfo.Email) : '',
                transacaoId: transacaoId
              };
            });

          this.filtrarDados();
          this.carregando = false;
        },
        error: (err: any) => {
          console.error('Erro ao carregar dados:', err);
          if (this.toast) {
            this.toast.show({
              title: 'Erro',
              content: 'Não foi possível carregar as etiquetas.',
              cssClass: 'e-toast-danger',
              icon: 'e-error toast-icons'
            });
          }
          this.carregando = false;
        }
      });
    });
  }

  // --- Funções omitidas foram substituídas. Vou recolocar o filtrarDados aqui só para o diff não quebrar
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

  // ─── Ações em Lote ─────────────────────────────────────────────────────────

  verificandoStatus = false;
  enviandoRastreiosLote = false;

  onVerificarStatusSelecionados(): void {
    const selecionados = this.grid.getSelectedRecords() as any[];
    if (selecionados.length === 0) return;

    const transacaoIds = selecionados
      .filter(e => e.transacaoId)
      .map(e => e.transacaoId!);

    if (transacaoIds.length === 0) {
      if (this.toast) {
        this.toast.show({ title: 'Atenção', content: 'Nenhum dos selecionados possui transação InfinitePay.', cssClass: 'e-toast-warning' });
      }
      return;
    }

    this.verificandoStatus = true;
    this.envioLoteService.verificarStatus(transacaoIds).subscribe({
      next: (resultados) => {
        resultados.forEach(res => {
          const envio = this.rawData.find(e => (e as any).transacaoId === res.transacaoId) as any;
          if (envio) {
            envio.statusPagamento = res.statusPagamento;
            envio.statusSuperfrete = res.statusSuperfrete;
            if (res.statusSuperfrete === 'Liberada') {
              envio.statusLabel = 'Aguardando Postagem';
              envio.statusClass = 'status-released';
            }
          }
        });
        this.filtrarDados(); // refresh grid
        this.grid.refresh();
        this.verificandoStatus = false;
        if (this.toast) this.toast.show({ title: 'Sucesso', content: 'Status atualizados!', cssClass: 'e-toast-success' });
      },
      error: (err) => {
        console.error('Erro ao verificar status:', err);
        this.verificandoStatus = false;
        if (this.toast) this.toast.show({ title: 'Erro', content: 'Falha ao sincronizar status.', cssClass: 'e-toast-danger' });
      }
    });
  }

  onEnviarRastreiosSelecionados(): void {
    const selecionados = this.grid.getSelectedRecords() as any[];
    const enviosParaRastreio = selecionados.filter(e => e.id && e.email);

    if (enviosParaRastreio.length === 0) {
      if (this.toast) this.toast.show({ title: 'Atenção', content: 'Nenhum selecionado possui etiqueta e e-mail.', cssClass: 'e-toast-warning' });
      return;
    }

    this.enviandoRastreiosLote = true;

    const payload = {
      envios: enviosParaRastreio.map(e => ({
        etiquetaId: e.id,
        email: e.email,
        nome: e.destinatario
      }))
    };

    this.envioLoteService.enviarRastreioLote(payload).subscribe({
      next: (res) => {
        this.enviandoRastreiosLote = false;
        if (res.totalErro > 0) {
          const nomesErros = res.erros.map((err: any) => `${err.nome} (${err.erro})`).join(', ');
          if (this.toast) this.toast.show({
            title: 'Atenção',
            content: `${res.totalSucesso} e-mail(s) enviado(s). Erro(s) em ${res.totalErro}: ${nomesErros}`,
            cssClass: 'e-toast-warning'
          });
        } else {
          if (this.toast) this.toast.show({ title: 'Sucesso', content: `${res.totalSucesso} e-mail(s) enviado(s)!`, cssClass: 'e-toast-success' });
        }
      },
      error: (err) => {
        console.error('Erro ao enviar rastreios:', err);
        this.enviandoRastreiosLote = false;
        if (this.toast) this.toast.show({ title: 'Erro', content: 'Falha ao enviar rastreios.', cssClass: 'e-toast-danger' });
      }
    });
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
