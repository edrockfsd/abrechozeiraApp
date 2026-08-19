import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { GridComponent, GridModule, SelectionSettingsModel } from '@syncfusion/ej2-angular-grids';
import { DateRangePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule, ComboBoxModule } from '@syncfusion/ej2-angular-dropdowns';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { FormsModule } from '@angular/forms';
import { VendasService, VendaListItem } from '../../../../services/vendas.service';
import { ArremateService, LiveCombo } from '../../../arremates/services/arremate.service';
import { NfceService } from '../../services/nfce.service';

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    GridModule,
    DateRangePickerModule,
    DropDownListModule,
    ComboBoxModule,
    ButtonModule
  ],
  templateUrl: './sales-list.component.html',
  styleUrls: ['./sales-list.component.scss']
})
export class SalesListComponent implements OnInit {
  @ViewChild('grid') grid!: GridComponent;

  vendas: VendaListItem[] = [];
  loading = false;
  loadingBatchNfce = false;
  range: any;
  status: string | null = null;
  selectedLiveId: number | null = null;
  lives: LiveCombo[] = [];

  mensagem: { tipo: 'success' | 'warning' | 'error'; texto: string } | null = null;

  selectionOptions: SelectionSettingsModel = {
    type: 'Multiple',
    checkboxOnly: true,
    persistSelection: true
  };

  statusOptions = [
    { text: 'Todos os Status', value: null },
    { text: 'Confirmada', value: 'Confirmada' },
    { text: 'Faturada', value: 'Faturada' },
    { text: 'Cancelada', value: 'Cancelada' }
  ];

  constructor(
    private vendasService: VendasService,
    private arremateService: ArremateService,
    private nfceService: NfceService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.carregarLives();

    this.route.queryParams.subscribe(params => {
      if (params['liveId']) {
        this.selectedLiveId = Number(params['liveId']);
      }
      this.load();
    });
  }

  carregarLives(): void {
    this.arremateService.getLivesCombo().subscribe({
      next: (data) => {
        this.lives = [{ id: 0, titulo: 'Todas as Lives' }, ...data];
      },
      error: (err) => console.error('Erro ao carregar lives:', err)
    });
  }

  private toIso(d: Date): string {
    return new Date(d).toISOString();
  }

  load(): void {
    this.loading = true;
    let startIso: string | undefined;
    let endIso: string | undefined;

    if (this.range?.startDate) startIso = this.toIso(this.range.startDate as Date);
    if (this.range?.endDate) endIso = this.toIso(this.range.endDate as Date);

    const liveIdFilter = this.selectedLiveId && this.selectedLiveId > 0 ? this.selectedLiveId : undefined;

    this.vendasService.getListagem({
      liveId: liveIdFilter,
      status: this.status || undefined,
      inicio: startIso,
      fim: endIso,
      limite: 150
    }).subscribe({
      next: (list) => {
        this.vendas = list || [];
        this.loading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar vendas:', err);
        this.vendas = [];
        this.loading = false;
      }
    });
  }

  emitirNfceIndividual(venda: VendaListItem): void {
    if (!venda?.id) return;
    this.mensagem = null;

    this.vendasService.emitirNfceLote([venda.id]).subscribe({
      next: (res) => {
        if (res.sucessos > 0) {
          const nf = res.resultados[0];
          this.mensagem = {
            tipo: 'success',
            texto: `NFC-e Nº ${nf.numero} emitida com sucesso para a venda #${venda.id}! Chave: ${nf.chaveAcesso}`
          };
        } else if (res.erros && res.erros.length > 0) {
          this.mensagem = {
            tipo: 'error',
            texto: `Erro ao emitir NFC-e: ${res.erros[0].erro}`
          };
        }
        this.load();
      },
      error: (err) => {
        this.mensagem = {
          tipo: 'error',
          texto: err.error?.erro || 'Erro ao emitir NFC-e'
        };
      }
    });
  }

  emitirNfceLote(): void {
    let selectedRecords = (this.grid?.getSelectedRecords() as VendaListItem[]) || [];
    
    if (selectedRecords.length === 0 && this.grid) {
      const selectedIndexes = this.grid.getSelectedRowIndexes();
      if (selectedIndexes && selectedIndexes.length > 0) {
        selectedRecords = selectedIndexes.map(idx => this.vendas[idx]).filter(Boolean);
      }
    }
    
    if (selectedRecords.length === 0) {
      this.mensagem = {
        tipo: 'warning',
        texto: 'Selecione pelo menos uma venda na tabela para emitir NFC-e em lote.'
      };
      return;
    }

    const vendaIds = selectedRecords.map(v => v.id);

    if (!confirm(`Deseja emitir NFC-e para as ${vendaIds.length} vendas selecionadas?`)) {
      return;
    }

    this.loadingBatchNfce = true;
    this.mensagem = null;

    this.vendasService.emitirNfceLote(vendaIds).subscribe({
      next: (res) => {
        this.loadingBatchNfce = false;
        if (res.totalErros === 0) {
          this.mensagem = {
            tipo: 'success',
            texto: `Sucesso! Todas as ${res.sucessos} NFC-es foram emitidas e autorizadas.`
          };
        } else {
          this.mensagem = {
            tipo: 'warning',
            texto: `Processamento em lote: ${res.sucessos} NFC-es emitidas com sucesso e ${res.totalErros} erros.`
          };
        }
        this.load();
      },
      error: (err) => {
        this.loadingBatchNfce = false;
        console.error('Erro na emissão em lote:', err);
        this.mensagem = {
          tipo: 'error',
          texto: err.error?.erro || 'Erro no processamento da emissão em lote.'
        };
      }
    });
  }

  copiarChave(chave: string): void {
    if (!chave) return;
    navigator.clipboard.writeText(chave).then(() => {
      this.mensagem = {
        tipo: 'success',
        texto: `Chave copiada: ${chave}`
      };
      setTimeout(() => {
        if (this.mensagem?.texto.includes(chave)) {
          this.mensagem = null;
        }
      }, 4000);
    }).catch(() => {
      prompt('Copie a chave da NFC-e:', chave);
    });
  }

  limparMensagem(): void {
    this.mensagem = null;
  }
}
