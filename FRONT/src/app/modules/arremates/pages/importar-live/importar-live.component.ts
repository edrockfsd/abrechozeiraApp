import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ComboBoxModule, FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import { GridComponent, GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastModule, ToastComponent } from '@syncfusion/ej2-angular-notifications';
import {
  ArremateService,
  LiveCombo,
  LinhaPreview,
  ResultadoImportacaoLive,
  DetalhePedidoLive,
  StatusLiveImportacao,
  MapeamentoCliente,
  ClienteCombo
} from '../../services/arremate.service';

export interface ClienteOption {
  id: number | null;
  label: string;
  nome: string;
  nick: string;
  cpf: string;
  searchStr: string;
  cpfDigits: string;
}

function normalizarParaBusca(texto: string): string {
  return (texto || '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase();
}

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

  // Mapeamento e Vinculação de Clientes
  mapeamentos: MapeamentoCliente[] = [];
  clientesCadastrados: ClienteCombo[] = [];
  clientesOptions: ClienteOption[] = [];
  public clienteFields: object = { text: 'label', value: 'id' };
  filtroMapeamento: 'todos' | 'vinculados' | 'parecidos' | 'novos' | 'desconsiderados' = 'todos';
  buscaComprador: string = '';
  
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
      // Carregar prévia e vínculos automaticamente ao selecionar arquivo
      this.carregarPreview();
    }
  }

  carregarPreview(): void {
    if (this.modoImportacao === 'url' && !this.googleSheetUrl.trim()) {
      this.showToast('warning', 'URL Obrigatória', 'Informe a URL da Google Sheet.');
      return;
    }

    if (this.modoImportacao === 'arquivo' && !this.selectedFile) {
      this.showToast('warning', 'Arquivo Obrigatório', 'Selecione um arquivo .xlsx para carregar a prévia.');
      return;
    }

    this.loadingPreview = true;
    this.previewData = null;
    this.mapeamentos = [];
    this.mensagem = null;

    const request$ = this.modoImportacao === 'url'
      ? this.arremateService.previewUrl(this.googleSheetUrl, this.sheetName)
      : this.arremateService.previewXlsx(this.selectedFile!);

    request$.subscribe({
      next: (res) => {
        this.previewData = res.linhas;
        this.previewResumo = {
          totalLinhas: res.totalLinhas,
          compradores: res.compradores
        };
        this.mapeamentos = res.mapeamentos || [];
        this.clientesCadastrados = res.clientes || [];

        // Montar opções com índice de busca (Google-like substring em qualquer parte de Nome, Nick ou CPF)
        this.clientesOptions = [
          {
            id: null,
            label: '➕ Criar Novo Cliente (Auto-cadastro)',
            nome: '',
            nick: '',
            cpf: '',
            searchStr: 'criar novo cliente auto cadastro novo',
            cpfDigits: ''
          },
          ...this.clientesCadastrados.map(c => {
            const cpfNum = (c.cpf || '').replace(/\D/g, '');
            const label = `${c.nome} (${c.nickName || 'sem @'})${c.cpf ? ' - CPF: ' + c.cpf : ''}`;
            const searchStr = normalizarParaBusca(`${c.nome} ${c.nickName || ''} ${c.cpf || ''} ${cpfNum}`);
            return {
              id: c.id,
              label,
              nome: c.nome,
              nick: c.nickName || '',
              cpf: c.cpf || '',
              searchStr,
              cpfDigits: cpfNum
            };
          })
        ];

        this.loadingPreview = false;
        const vinculados = this.totalVinculados;
        const novos = this.totalNovos;
        this.showToast(
          'success',
          'Prévia Carregada',
          `${res.totalLinhas} peças e ${res.compradores} compradores encontrados (${vinculados} vinculados, ${novos} novos).`
        );
      },
      error: (err) => {
        this.loadingPreview = false;
        console.error('Erro ao carregar prévia:', err);
        const erroMsg = err.error?.erro || 'Erro ao ler os dados da planilha.';
        this.showToast('error', 'Erro na Prévia', erroMsg);
      }
    });
  }

  // Busca inteligente tipo Google no Dropdown (filtra por qualquer termo em nome, sobrenome, nick ou CPF)
  onFilteringCliente(e: FilteringEventArgs): void {
    const rawText = (e.text || '').trim();
    e.preventDefaultAction = true;

    if (!rawText) {
      e.updateData(this.clientesOptions as any);
      return;
    }

    const tokens = normalizarParaBusca(rawText).split(/\s+/).filter(t => t.length > 0);
    const cleanDigits = rawText.replace(/\D/g, '');

    const filtered = this.clientesOptions.filter(opt => {
      if (!opt.id) return false;
      // Todos os tokens digitados precisam existir em qualquer parte da string de busca
      const allTokensMatch = tokens.every(token => opt.searchStr.includes(token));
      if (allTokensMatch) return true;

      // Busca por dígitos de CPF
      if (cleanDigits.length >= 3 && opt.cpfDigits && opt.cpfDigits.includes(cleanDigits)) {
        return true;
      }

      return false;
    });

    e.updateData(filtered as any);
  }

  // Alternar desconsiderar comprador (desistência de compra)
  toggleDesconsiderarComprador(item: MapeamentoCliente): void {
    item.ignorado = !item.ignorado;
    if (item.ignorado) {
      this.showToast('warning', 'Comprador Desconsiderado', `@${item.compradorPlanilha} (${item.totalPecas} peças) não será importado.`);
    } else {
      this.showToast('success', 'Comprador Restaurado', `@${item.compradorPlanilha} foi restaurado para a importação.`);
    }
  }

  // Getters para a UI de Mapeamento
  get totalAtivos(): number {
    return this.mapeamentos.filter(m => !m.ignorado).length;
  }

  get totalIgnorados(): number {
    return this.mapeamentos.filter(m => !!m.ignorado).length;
  }

  get totalVinculados(): number {
    return this.mapeamentos.filter(m => !m.ignorado && m.pessoaId !== null && m.pessoaId !== undefined && m.tipoMatch !== 'SIMILAR').length;
  }

  get totalParecidos(): number {
    return this.mapeamentos.filter(m => !m.ignorado && m.pessoaId !== null && m.pessoaId !== undefined && m.tipoMatch === 'SIMILAR').length;
  }

  get totalNovos(): number {
    return this.mapeamentos.filter(m => !m.ignorado && (m.pessoaId === null || m.pessoaId === undefined)).length;
  }

  get totalPecasImportar(): number {
    return this.mapeamentos.filter(m => !m.ignorado).reduce((acc, m) => acc + m.totalPecas, 0);
  }

  get totalPecasIgnoradas(): number {
    return this.mapeamentos.filter(m => !!m.ignorado).reduce((acc, m) => acc + m.totalPecas, 0);
  }

  get mapeamentosFiltrados(): MapeamentoCliente[] {
    let lista = this.mapeamentos;

    if (this.filtroMapeamento === 'vinculados') {
      lista = lista.filter(m => !m.ignorado && m.pessoaId !== null && m.pessoaId !== undefined && m.tipoMatch !== 'SIMILAR');
    } else if (this.filtroMapeamento === 'parecidos') {
      lista = lista.filter(m => !m.ignorado && m.pessoaId !== null && m.pessoaId !== undefined && m.tipoMatch === 'SIMILAR');
    } else if (this.filtroMapeamento === 'novos') {
      lista = lista.filter(m => !m.ignorado && (m.pessoaId === null || m.pessoaId === undefined));
    } else if (this.filtroMapeamento === 'desconsiderados') {
      lista = lista.filter(m => !!m.ignorado);
    }

    if (this.buscaComprador.trim()) {
      const q = normalizarParaBusca(this.buscaComprador.trim());
      lista = lista.filter(m =>
        normalizarParaBusca(m.compradorPlanilha).includes(q) ||
        (m.sugeridoNome && normalizarParaBusca(m.sugeridoNome).includes(q)) ||
        (m.sugeridoNick && normalizarParaBusca(m.sugeridoNick).includes(q)) ||
        (m.sugeridoCpf && m.sugeridoCpf.includes(q))
      );
    }

    return lista;
  }

  onClienteChange(item: MapeamentoCliente, event: any): void {
    const novoId = event.value ? Number(event.value) : null;
    item.pessoaId = novoId;

    if (novoId) {
      const cliente = this.clientesCadastrados.find(c => c.id === novoId);
      if (cliente) {
        item.sugeridoNome = cliente.nome;
        item.sugeridoNick = cliente.nickName;
        item.sugeridoCpf = cliente.cpf;
        item.tipoMatch = item.sugeridoPessoaId === novoId ? (item.tipoMatch || 'EXATO') : 'MANUAL';
      }
    } else {
      item.sugeridoNome = null;
      item.sugeridoNick = null;
      item.sugeridoCpf = null;
      item.tipoMatch = 'NOVO';
    }
  }

  desvincularCliente(item: MapeamentoCliente): void {
    item.pessoaId = null;
    item.sugeridoNome = null;
    item.sugeridoNick = null;
    item.sugeridoCpf = null;
    item.tipoMatch = 'NOVO';
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
1. Cadastrar as peças como Produtos (${this.totalPecasImportar} peças)
2. Gerar registros de Arremates
3. Criar Pedidos e Vendas vinculando ${this.totalVinculados + this.totalParecidos} clientes existentes e criando ${this.totalNovos} novos
${this.totalIgnorados > 0 ? `4. Desconsiderar ${this.totalIgnorados} comprador(es) (${this.totalPecasIgnoradas} peças) por desistência\n` : ''}5. Deixar pronto para posterior emissão de NFC-e`;

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
      this.arremateService.importarPlanilhaUrl(this.selectedLiveId, this.googleSheetUrl, this.sheetName, this.mapeamentos).subscribe({
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
      this.arremateService.importarPlanilhaXlsx(this.selectedFile, this.selectedLiveId, this.mapeamentos).subscribe({
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
