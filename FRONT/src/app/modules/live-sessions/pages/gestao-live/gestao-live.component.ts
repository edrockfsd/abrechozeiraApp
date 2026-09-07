import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subscription } from 'rxjs';
import {
  LiveTrackerService,
  MatchDto,
  EstadoRastreioDto,
  ComentarioLiveDto,
  AuditoriaMatchDto
} from '../../services/live-tracker.service';
import { environment } from '../../../../../environments/environment';

import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-gestao-live',
  templateUrl: './gestao-live.component.html',
  styleUrls: ['./gestao-live.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ]
})
export class GestaoLiveComponent implements OnInit, OnDestroy {
  @ViewChild('inputCodigoEl') inputCodigoEl?: ElementRef<HTMLInputElement>;
  @ViewChild('inputDescricaoEl') inputDescricaoEl?: ElementRef<HTMLInputElement>;
  @ViewChild('inputValorEl') inputValorEl?: ElementRef<HTMLInputElement>;
  @ViewChild('chatFeedEl') chatFeedEl?: ElementRef<HTMLDivElement>;

  public liveId: number = 0;
  public liveVideoId: number = 0;
  public liveTitulo: string = 'Live Instagram';
  public googleSheetUrl: string = '';
  public sheetName: string = 'vendas';
  public totalComentarios: number = 0;
  public isSignalRConnected: boolean = false;

  // Form
  public inputCodigo: string = '';
  public inputDescricao: string = '';
  public inputValor: number | null = null;

  // Estado de Rastreamento Ativo
  public isTracking: boolean = false;
  public activeCode: string = '';
  public activeDescription: string = '';
  public activeValue: number = 0;
  public matches: MatchDto[] = [];
  public comprador: MatchDto | null = null;
  public fila: MatchDto[] = [];

  // Chat Feed
  public chatMessages: ComentarioLiveDto[] = [];
  public autoScrollChat: boolean = true;

  // Histórico de Arremates
  public historico: any[] = [];
  public totalFaturado: number = 0;

  // Auditoria Modal
  public modalAuditoriaAberta: boolean = false;
  public auditoriaCodigo: string = '';
  public auditoriaMatches: AuditoriaMatchDto[] = [];
  public carregandoAuditoria: boolean = false;

  // Configurações e Contingência
  public painelConfigAberto: boolean = false;
  public ssnHabilitado: boolean = false;
  public ssnSessionId: string = '';
  public ssnSocket?: WebSocket;
  public ssnStatus: string = 'Desconectado';

  public sincronizandoPlanilha: boolean = false;
  public salvandoConfigPlanilha: boolean = false;

  // Alerta Toast
  public toastMensagem: string = '';
  public toastTipo: 'sucesso' | 'erro' | 'info' | 'match' = 'info';
  public toastVisivel: boolean = false;
  private toastTimer?: any;

  private subs: Subscription = new Subscription();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    public trackerService: LiveTrackerService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.liveId = Number(params['id'] || params['liveId'] || 0);
      this.carregarDadosLive();
    });
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    this.trackerService.desconectar(this.liveId);
    if (this.ssnSocket) {
      try { this.ssnSocket.close(); } catch {}
    }
  }

  private carregarDadosLive(): void {
    if (!this.liveId) return;

    // Buscar informações completas e contextualizadas da Live (incluindo o LiveVideoId correto e total de comentários)
    this.http.get<any>(`${environment.apiUrl}/LiveTracker/info/${this.liveId}`).subscribe({
      next: info => {
        this.liveTitulo = info.titulo || `Live #${this.liveId}`;
        this.googleSheetUrl = info.googleSheetUrl || '';
        this.liveVideoId = info.liveVideoId || 0;
        this.totalComentarios = info.totalComentarios || 0;

        // Se a live não tiver planilha configurada, herda a última do sistema ou a padrão
        if (!this.googleSheetUrl) {
          this.http.get<any[]>(`${environment.apiUrl}/Lives`).subscribe({
            next: lives => {
              const comPlanilha = lives?.find(l => l.googleSheetUrl);
              const urlDefault = comPlanilha?.googleSheetUrl || 'https://docs.google.com/spreadsheets/d/1HUEcIGWlgdcMuBi1zIhYX4sm660UT_ttyUkb_XhAS3o/edit?gid=1053114646#gid=1053114646';
              this.googleSheetUrl = urlDefault;
              this.trackerService.configurarPlanilha(this.liveId, urlDefault).subscribe();
            }
          });
        }

        this.iniciarConexaoEAssinaturas();
      },
      error: () => {
        // Fallback para API de Lives básica caso falhe
        this.http.get<any>(`${environment.apiUrl}/Lives/${this.liveId}`).subscribe({
          next: live => {
            this.liveTitulo = live.titulo || `Live #${this.liveId}`;
            this.googleSheetUrl = live.googleSheetUrl || '';
            this.iniciarConexaoEAssinaturas();
          },
          error: () => {
            this.iniciarConexaoEAssinaturas();
          }
        });
      }
    });

    // Carregar histórico de arremates da live
    this.carregarHistorico();
  }

  private iniciarConexaoEAssinaturas(): void {
    this.trackerService.iniciarConexao(this.liveId, this.liveVideoId);

    // Status da conexão SignalR
    this.subs.add(
      this.trackerService.conexaoStatus$.subscribe(status => {
        this.isSignalRConnected = status;
      })
    );

    // Estado de rastreamento inicial
    this.trackerService.obterEstadoAtual(this.liveId).subscribe({
      next: estado => this.atualizarEstadoRastreio(estado),
      error: () => {}
    });

    // Atualização do Estado do Rastreio via SignalR
    this.subs.add(
      this.trackerService.statusRastreio$.subscribe(estado => {
        if (estado) this.atualizarEstadoRastreio(estado);
      })
    );

    // Novo Comentário no Chat
    this.subs.add(
      this.trackerService.novoComentario$.subscribe(msg => {
        if (!msg) return;
        // Evita mensagens duplicadas no feed visual do chat
        if (this.chatMessages.some(m => (m.id && msg.id && m.id === msg.id) || (m.username === msg.username && m.commentText === msg.commentText && m.commentTimestamp === msg.commentTimestamp))) {
          return;
        }
        this.totalComentarios++;
        this.chatMessages.push(msg);
        if (this.chatMessages.length > 300) {
          this.chatMessages.shift();
        }
        if (this.autoScrollChat) {
          setTimeout(() => this.scrollChatParaFim(), 50);
        }
      })
    );

    // Match Detectado
    this.subs.add(
      this.trackerService.matchDetectado$.subscribe(match => {
        if (!match) return;
        if (this.matches.some(m => m.username.toLowerCase() === match.username.toLowerCase())) {
          return;
        }
        this.matches.push(match);
        this.separarCompradorEFila();
        const texto = match.posicao === 1
          ? `🏆 COMPRADOR: @${match.username} (${match.commentTimestampFmt})`
          : `📋 FILA ${match.posicao - 1}: @${match.username}`;
        this.mostrarToast(texto, 'match');
      })
    );

    // Matches Atualizados (ex: remoção)
    this.subs.add(
      this.trackerService.matchesAtualizados$.subscribe(matches => {
        this.matches = matches;
        this.separarCompradorEFila();
      })
    );
  }

  private atualizarEstadoRastreio(estado: EstadoRastreioDto): void {
    this.isTracking = estado.isTracking;
    this.activeCode = estado.activeCode || '';
    this.activeDescription = estado.activeDescription || '';
    this.activeValue = estado.activeValue || 0;
    this.matches = estado.matches || [];
    this.separarCompradorEFila();
  }

  private separarCompradorEFila(): void {
    this.comprador = this.matches.find(m => m.posicao === 1) || null;
    this.fila = this.matches.filter(m => m.posicao > 1);
  }

  public carregarHistorico(): void {
    this.trackerService.obterArrematesDaLive(this.liveId).subscribe({
      next: data => {
        this.historico = data || [];
        this.totalFaturado = this.historico.reduce((acc, curr) => acc + (curr.valorArremate || 0), 0);
      },
      error: () => {}
    });
  }

  // ===== Ações do Operador =====

  public iniciarRastreio(): void {
    const codigo = this.inputCodigo.trim();
    if (!codigo) {
      this.mostrarToast('Por favor, informe o código da peça a rastrear.', 'erro');
      this.focarCodigo();
      return;
    }

    const req = {
      liveId: this.liveId,
      liveVideoId: this.liveVideoId,
      codigo: codigo,
      descricao: this.inputDescricao.trim(),
      valor: this.inputValor || 0
    };

    this.trackerService.iniciarRastreio(req).subscribe({
      next: estado => {
        this.atualizarEstadoRastreio(estado);
        this.mostrarToast(`Rastreando código "${codigo}"... Varredura retroativa executada!`, 'info');
      },
      error: err => {
        this.mostrarToast(`Erro ao iniciar rastreio: ${err.message}`, 'erro');
      }
    });
  }

  public pararRastreio(): void {
    this.trackerService.pararRastreio(this.liveId).subscribe({
      next: estado => {
        this.atualizarEstadoRastreio(estado);
        this.mostrarToast('Rastreio pausado.', 'info');
      },
      error: err => {
        this.mostrarToast(`Erro: ${err.message}`, 'erro');
      }
    });
  }

  public removerMatch(index: number): void {
    this.trackerService.removerMatch(this.liveId, index).subscribe({
      next: estado => {
        this.atualizarEstadoRastreio(estado);
        this.mostrarToast('Participante removido da lista.', 'info');
      },
      error: err => {
        this.mostrarToast(`Erro ao remover: ${err.message}`, 'erro');
      }
    });
  }

  public confirmarArremate(): void {
    if (!this.activeCode) {
      this.mostrarToast('Nenhuma peça ativa sendo rastreada.', 'erro');
      return;
    }

    const compradorHoraTexto = this.comprador
      ? `${this.comprador.commentTimestampFmt} - "${this.comprador.commentText}"`
      : undefined;

    const req = {
      liveId: this.liveId,
      codigo: this.activeCode,
      descricao: this.activeDescription,
      valor: this.activeValue,
      arrematante: this.comprador ? this.comprador.username : 'Sem Comprador',
      compradorHoraTexto: compradorHoraTexto,
      matches: this.matches,
      googleSheetUrl: this.googleSheetUrl,
      sheetName: this.sheetName
    };

    this.trackerService.confirmarArremate(req).subscribe({
      next: res => {
        let msg = `Peça "${res.codigo}" salva com sucesso! Comprador: @${res.comprador}`;
        if (res.googleSheetSincronizado) {
          msg += ' (Planilha Sheets atualizada ✅)';
        }
        this.mostrarToast(msg, 'sucesso');

        // Limpa campos para próxima peça
        this.inputCodigo = '';
        this.inputDescricao = '';
        this.inputValor = null;
        this.isTracking = false;
        this.activeCode = '';
        this.matches = [];
        this.separarCompradorEFila();

        this.carregarHistorico();
        this.focarCodigo();
      },
      error: err => {
        this.mostrarToast(`Erro ao salvar arremate: ${err.message}`, 'erro');
      }
    });
  }

  public limparCampos(): void {
    this.inputCodigo = '';
    this.inputDescricao = '';
    this.inputValor = null;
    this.focarCodigo();
  }

  public focarCodigo(): void {
    setTimeout(() => {
      this.inputCodigoEl?.nativeElement.focus();
    }, 100);
  }

  public focarDescricao(): void {
    setTimeout(() => {
      this.inputDescricaoEl?.nativeElement.focus();
    }, 50);
  }

  public focarValor(): void {
    setTimeout(() => {
      this.inputValorEl?.nativeElement.focus();
    }, 50);
  }

  // ===== Auditoria e Comprovante do Chat =====

  public abrirAuditoria(codigo?: string): void {
    const cod = codigo || this.activeCode || this.inputCodigo;
    if (!cod) {
      this.mostrarToast('Informe ou selecione um código para ver a auditoria.', 'erro');
      return;
    }

    this.auditoriaCodigo = cod;
    this.carregandoAuditoria = true;
    this.modalAuditoriaAberta = true;

    this.trackerService.obterAuditoria(this.liveId, cod, this.liveVideoId).subscribe({
      next: data => {
        if (data && data.length > 0) {
          this.auditoriaMatches = data;
        } else if (this.activeCode === cod && this.matches.length > 0) {
          this.auditoriaMatches = this.matches.map(m => ({
            posicao: m.posicao,
            username: m.username,
            textoDigitado: m.commentText,
            timestamp: m.commentTimestamp,
            timestampFmt: m.commentTimestampFmt,
            status: m.posicao === 1 ? '🏆 Comprador' : `📋 Fila ${m.posicao - 1}`
          }));
        } else {
          this.auditoriaMatches = [];
        }
        this.carregandoAuditoria = false;
      },
      error: err => {
        this.carregandoAuditoria = false;
        this.mostrarToast(`Erro na auditoria: ${err.message}`, 'erro');
      }
    });
  }

  public fecharAuditoria(): void {
    this.modalAuditoriaAberta = false;
  }

  public copiarComprovanteAuditoria(): void {
    if (this.auditoriaMatches.length === 0) return;

    let texto = `📋 *COMPROVANTE OFICIAL DA LIVE - CÓDIGO ${this.auditoriaCodigo}*\n`;
    texto += `Live: ${this.liveTitulo}\n`;
    texto += `Horário do Chat com precisão de milissegundos:\n\n`;

    this.auditoriaMatches.forEach(m => {
      const icon = m.posicao === 1 ? '🏆' : `[${m.posicao}º]`;
      texto += `${icon} @${m.username} -> ${m.timestampFmt} ("${m.textoDigitado}")\n`;
    });

    texto += `\n*Ordem cronológica exata registrada pelos servidores do Instagram.*`;

    navigator.clipboard.writeText(texto).then(() => {
      this.mostrarToast('Comprovante copiado para a área de transferência!', 'sucesso');
    }).catch(() => {
      this.mostrarToast('Não foi possível copiar automaticamente.', 'erro');
    });
  }

  // ===== Exportações e Contingência =====

  public exportarCSV(): void {
    if (this.historico.length === 0) {
      this.mostrarToast('Nenhum arremate registrado para exportar.', 'info');
      return;
    }

    const headers = ['Externo', 'Descricao', 'Valor', 'Comprador', 'Horario', 'Fila'];
    const rows = [headers.join(';')];

    this.historico.forEach(p => {
      const dataHora = p.dataArremate ? new Date(p.dataArremate).toLocaleTimeString('pt-BR') : '';
      const filaFormatada = (p.fila || '').replace(/;/g, ' ');
      rows.push([
        p.codigoLive || '',
        `"${p.produtoDescricao || ''}"`,
        (p.valorArremate || 0).toFixed(2),
        p.arrematante || '',
        dataHora,
        `"${filaFormatada}"`
      ].join(';'));
    });

    const csvContent = '\uFEFF' + rows.join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `arremates_live_${this.liveId}.csv`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    this.mostrarToast('Arquivo CSV baixado com sucesso!', 'sucesso');
  }

  public copiarParaClipboard(): void {
    if (this.historico.length === 0) {
      this.mostrarToast('Nenhum arremate para copiar.', 'info');
      return;
    }

    const headers = ['Externo', 'Descricao', 'Valor', 'Comprador', 'Fila'];
    const rows = [headers.join('\t')];

    this.historico.forEach(p => {
      rows.push([
        p.codigoLive || '',
        p.produtoDescricao || '',
        (p.valorArremate || 0).toFixed(2),
        p.arrematante || '',
        p.fila || ''
      ].join('\t'));
    });

    navigator.clipboard.writeText(rows.join('\n')).then(() => {
      this.mostrarToast('Tabela copiada! Agora basta dar Ctrl+V no Google Sheets.', 'sucesso');
    }).catch(() => {
      this.mostrarToast('Erro ao copiar tabela.', 'erro');
    });
  }

  public salvarConfiguracaoPlanilha(): void {
    if (!this.googleSheetUrl.trim()) {
      this.mostrarToast('Informe a URL da planilha Google.', 'erro');
      return;
    }

    this.salvandoConfigPlanilha = true;
    this.trackerService.configurarPlanilha(this.liveId, this.googleSheetUrl.trim()).subscribe({
      next: () => {
        this.salvandoConfigPlanilha = false;
        this.mostrarToast('Planilha vinculada à Live com sucesso!', 'sucesso');
      },
      error: () => {
        this.salvandoConfigPlanilha = false;
        this.mostrarToast('Erro ao salvar configuração da planilha.', 'erro');
      }
    });
  }

  public sincronizarPlanilha(): void {
    if (this.historico.length === 0) {
      this.mostrarToast('Nenhum arremate gravado nesta live para sincronizar.', 'info');
      return;
    }

    this.sincronizandoPlanilha = true;
    this.trackerService.sincronizarPlanilhaRetroativo(this.liveId, this.googleSheetUrl, this.sheetName).subscribe({
      next: (res) => {
        this.sincronizandoPlanilha = false;
        if (res.sucesso) {
          if (res.googleSheetUrl && !this.googleSheetUrl) {
            this.googleSheetUrl = res.googleSheetUrl;
          }
          this.mostrarToast(res.mensagem || `${res.total} arremate(s) sincronizado(s) com a planilha!`, 'sucesso');
        } else {
          this.mostrarToast(res.mensagem || 'Falha ao sincronizar com o Google Sheets.', 'erro');
        }
      },
      error: (err) => {
        this.sincronizandoPlanilha = false;
        this.mostrarToast(`Erro ao sincronizar planilha: ${err.message}`, 'erro');
      }
    });
  }

  public concluirLiveEProcessar(): void {
    if (confirm(`Deseja concluir a Live "${this.liveTitulo}" e prosseguir para faturar/gerar pedidos das peças arrematadas?`)) {
      this.router.navigate(['/lives/contabilizacao', this.liveId]);
    }
  }

  // ===== Social Stream Ninja (Backup) =====

  public toggleSSN(): void {
    if (this.ssnSocket) {
      this.ssnSocket.close();
      this.ssnSocket = undefined;
      this.ssnStatus = 'Desconectado';
      this.mostrarToast('Social Stream Ninja desconectado.', 'info');
    } else {
      if (!this.ssnSessionId) {
        this.mostrarToast('Informe o Session ID do Social Stream Ninja.', 'erro');
        return;
      }
      this.conectarAoSSN(this.ssnSessionId.trim());
    }
  }

  private conectarAoSSN(sessionId: string): void {
    const url = `wss://io.socialstream.ninja/join/${sessionId}/4`;
    this.ssnStatus = 'Conectando...';

    this.ssnSocket = new WebSocket(url);
    this.ssnSocket.onopen = () => {
      this.ssnStatus = 'Conectado (Canal 4)';
      this.mostrarToast('SSN Conectado com sucesso!', 'sucesso');
    };

    this.ssnSocket.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (data.chatname && data.chatmessage !== undefined) {
          // Injeta a mensagem no backend
          this.trackerService.injetarComentarioSSN({
            liveId: this.liveId,
            chatName: data.chatname,
            chatMessage: data.chatmessage,
            platform: data.type || 'instagram'
          }).subscribe();
        }
      } catch {}
    };

    this.ssnSocket.onclose = () => {
      this.ssnStatus = 'Desconectado';
    };

    this.ssnSocket.onerror = () => {
      this.ssnStatus = 'Erro de Conexão';
      this.mostrarToast('Erro ao conectar ao Social Stream Ninja.', 'erro');
    };
  }

  // ===== Helpers UI =====

  public isComentarioMatch(texto: string): boolean {
    if (!this.isTracking || !this.activeCode || !texto) return false;
    const t = texto.trim();
    const c = this.activeCode.trim();
    return t.toLowerCase() === c.toLowerCase();
  }

  private scrollChatParaFim(): void {
    if (this.chatFeedEl?.nativeElement) {
      const el = this.chatFeedEl.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
  }

  public mostrarToast(msg: string, tipo: 'sucesso' | 'erro' | 'info' | 'match'): void {
    this.toastMensagem = msg;
    this.toastTipo = tipo;
    this.toastVisivel = true;

    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toastTimer = setTimeout(() => {
      this.toastVisivel = false;
    }, 4500);
  }

  public voltar(): void {
    this.router.navigate(['/lives']);
  }
}
