import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ComboBoxModule } from '@syncfusion/ej2-angular-dropdowns';
import { GridComponent, GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastModule, ToastComponent } from '@syncfusion/ej2-angular-notifications';
import {
  ArremateService,
  LiveCombo,
  LinhaPreview,
  ResultadoImportacaoLive,
  DetalhePedidoLive,
  StatusLiveImportacao
} from '../../services/arremate.service';

@Component({
  selector: 'app-importar-live',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    ComboBoxModule,
    GridModule,
    ButtonModule,
    ToastModule
  ],
  templateUrl: './importar-live.component.html',
  styleUrls: ['./importar-live.component.scss']
})
export class ImportarLiveComponent implements OnInit {
  @ViewChild('toast') public toast!: ToastComponent;
  @ViewChild('gridPreview') public gridPreview?: GridComponent;
  @ViewChild('gridResult') public gridResult?: GridComponent;

  public toastPosition = { X: 'Right', Y: 'Top' };
  public toastWidth = '420';

  lives: LiveCombo[] = [];
  selectedLiveId: number | null = null;
  public liveFields: object = { text: 'titulo', value: 'id' };

  // Status da Live Selecionada (regras de bloqueio e substituição)
  statusLive: StatusLiveImportacao | null = null;
  loadingStatusLive: boolean = false;

  // Modo de importação
  modoImportacao: 'url' | 'arquivo' = 'url';
  
  // URL padrão da planilha de live
  googleSheetUrl: string = 'https://docs.google.com/spreadsheets/d/1HUEcIGWlgdcMuBi1zIhYX4sm660UT_ttyUkb_XhAS3o/edit?gid=1053114646#gid=1053114646';
  sheetName: string = 'vendas';
  
  // Arquivo upload
  selectedFile: File | null = null;
  selectedFileName: string = '';

  // Estados de carregamento
  loadingPreview: boolean = false;
  loadingImport: boolean = false;
  
  // Dados de preview e resultado
  previewData: LinhaPreview[] | null = null;
  previewResumo: { totalLinhas: number; compradores: number } | null = null;
  resultadoImportacao: ResultadoImportacaoLive | null = null;
  
  mensagem: { tipo: 'success' | 'warning' | 'error'; titulo: string; texto: string } | null = null;

  constructor(private arremateService: ArremateService) {}

  ngOnInit(): void {
    this.carregarLives();
  }

  carregarLives(): void {
    this.arremateService.getLivesCombo().subscribe({
      next: (data) => {
        this.lives = data;
        if (data.length > 0 && !this.selectedLiveId) {
          this.selectedLiveId = data[0].id;
          this.carregarStatusLive(this.selectedLiveId);
        }
      },
      error: (err) => {
        console.error('Erro ao carregar lives:', err);
        this.showToast('error', 'Erro', 'Não foi possível carregar a lista de lives.');
      }
    });
  }

  onLiveChange(event: any): void {
    this.selectedLiveId = event.value;
    if (this.selectedLiveId) {
      this.carregarStatusLive(this.selectedLiveId);
    } else {
      this.statusLive = null;
    }
  }

  carregarStatusLive(liveId: number): void {
    this.loadingStatusLive = true;
    this.statusLive = null;
    this.arremateService.getStatusLive(liveId).subscribe({
      next: (status) => {
        this.statusLive = status;
        this.loadingStatusLive = false;
        if (status.bloqueadoParaImportacao) {
          this.showToast('error', 'Live Bloqueada para Reimportação', status.motivoBloqueio || 'Esta live possui NFC-e autorizada.');
        }
      },
      error: (err) => {
        this.loadingStatusLive = false;
        console.warn('Não foi possível verificar o status prévio da live:', err);
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files?.[0];
    if (file) {
      if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.xls')) {
        this.showToast('warning', 'Formato Inválido', 'Por favor, selecione um arquivo Excel (.xlsx).');
        return;
      }
      this.selectedFile = file;
      this.selectedFileName = file.name;
    }
  }

  carregarPreview(): void {
    if (this.modoImportacao === 'url') {
      if (!this.googleSheetUrl.trim()) {
        this.showToast('warning', 'URL Obrigatória', 'Informe a URL da Google Sheet.');
        return;
      }

      this.loadingPreview = true;
      this.previewData = null;
      this.mensagem = null;

      this.arremateService.previewUrl(this.googleSheetUrl, this.sheetName).subscribe({
        next: (res) => {
          this.previewData = res.linhas;
          this.previewResumo = {
            totalLinhas: res.totalLinhas,
            compradores: res.compradores
          };
          this.loadingPreview = false;
          this.showToast('success', 'Preview Carregado', `${res.totalLinhas} linhas encontradas com ${res.compradores} compradores.`);
        },
        error: (err) => {
          this.loadingPreview = false;
          console.error('Erro ao carregar preview:', err);
          const erroMsg = err.error?.erro || 'Erro ao ler os dados da planilha via URL.';
          this.showToast('error', 'Erro no Preview', erroMsg);
        }
      });
    } else {
      this.showToast('info', 'Importação Direta', 'Para arquivos locais (.xlsx), clique diretamente em "Processar e Importar".');
    }
  }

  executarImportacao(): void {
    if (!this.selectedLiveId) {
      this.showToast('warning', 'Live Obrigatória', 'Selecione a Live correspondente aos arremates.');
      return;
    }

    if (this.statusLive?.bloqueadoParaImportacao) {
      this.showToast('error', 'Importação Bloqueada', this.statusLive.motivoBloqueio || 'Esta live possui notas fiscais autorizadas na SEFAZ.');
      return;
    }

    if (this.modoImportacao === 'url' && !this.googleSheetUrl.trim()) {
      this.showToast('warning', 'URL Obrigatória', 'Informe o link da Google Sheet.');
      return;
    }

    if (this.modoImportacao === 'arquivo' && !this.selectedFile) {
      this.showToast('warning', 'Arquivo Obrigatório', 'Selecione um arquivo .xlsx para upload.');
      return;
    }

    let confirmMsg = `Confirma a importação dos arremates da live selecionada?
Isso irá:
1. Cadastrar as peças como Produtos (desmembradas por IA)
2. Gerar registros de Arremates
3. Criar Pedidos e Vendas para cada cliente
4. Vincular para posterior emissão de NFC-e`;

    if (this.statusLive && this.statusLive.totalVendas > 0) {
      confirmMsg = `⚠️ ATENÇÃO: Esta Live já possui ${this.statusLive.totalVendas} vendas geradas anteriormente (sem NFC-e autorizada).

Ao confirmar, os dados anteriores da Live serão substituídos com os novos dados desta planilha.

Deseja continuar com a substituição?`;
    } else if (this.statusLive && this.statusLive.totalArrematesProvisorios > 0) {
      confirmMsg = `ℹ️ Esta Live possui ${this.statusLive.totalArrematesProvisorios} arremates provisórios sincronizados durante a transmissão online.

Eles serão substituídos automaticamente pelos produtos e vendas oficiais desta planilha final.

Deseja iniciar o processamento?`;
    }

    if (!confirm(confirmMsg)) return;

    this.loadingImport = true;
    this.resultadoImportacao = null;
    this.mensagem = null;

    if (this.modoImportacao === 'url') {
      this.arremateService.importarPlanilhaUrl(this.selectedLiveId, this.googleSheetUrl, this.sheetName).subscribe({
        next: (res) => {
          this.resultadoImportacao = res;
          this.loadingImport = false;
          this.showToast('success', 'Importação Concluída!', `${res.produtosCadastrados} produtos cadastrados, ${res.pedidosGerados} pedidos e ${res.vendasGeradas} vendas geradas.`);
          if (this.selectedLiveId) this.carregarStatusLive(this.selectedLiveId);
        },
        error: (err) => {
          this.loadingImport = false;
          console.error('Erro na importação:', err);
          const msg = err.error?.erro || 'Erro durante o processamento da importação.';
          this.showToast('error', 'Erro na Importação', msg);
          if (this.selectedLiveId) this.carregarStatusLive(this.selectedLiveId);
        }
      });
    } else if (this.selectedFile) {
      this.arremateService.importarPlanilhaXlsx(this.selectedFile, this.selectedLiveId).subscribe({
        next: (res) => {
          this.resultadoImportacao = res;
          this.loadingImport = false;
          this.showToast('success', 'Importação Concluída!', `${res.produtosCadastrados} produtos cadastrados, ${res.pedidosGerados} pedidos e ${res.vendasGeradas} vendas geradas.`);
          if (this.selectedLiveId) this.carregarStatusLive(this.selectedLiveId);
        },
        error: (err) => {
          this.loadingImport = false;
          console.error('Erro na importação:', err);
          const msg = err.error?.erro || 'Erro durante o processamento da importação.';
          this.showToast('error', 'Erro na Importação', msg);
          if (this.selectedLiveId) this.carregarStatusLive(this.selectedLiveId);
        }
      });
    }
  }

  showToast(severity: 'success' | 'warning' | 'error' | 'info', title: string, message: string): void {
    if (this.toast) {
      this.toast.show({
        title,
        content: message,
        cssClass: `e-toast-${severity}`,
        icon: severity === 'error' ? 'e-error toast-icons' : (severity === 'warning' ? 'e-warning toast-icons' : 'e-success toast-icons'),
        position: this.toastPosition,
        timeOut: 6000,
        showCloseButton: true
      });
    }
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value || 0);
  }
}
