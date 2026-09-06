import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../../environments/environment';

export interface MatchDto {
  username: string;
  commentText: string;
  commentTimestamp: string;
  commentTimestampFmt: string; // HH:mm:ss.fff (com milissegundos)
  posicao: number; // 1 = Comprador, 2+ = Fila
  instagramCommentId?: string;
  isRetroativo: boolean;
}

export interface EstadoRastreioDto {
  liveId: number;
  isTracking: boolean;
  activeCode?: string | null;
  activeDescription?: string | null;
  activeValue?: number | null;
  matches: MatchDto[];
}

export interface ComentarioLiveDto {
  id: number;
  username: string;
  commentText: string;
  commentTimestamp: string;
  createdAt: string;
}

export interface IniciarRastreioRequest {
  liveId: number;
  liveVideoId?: number | null;
  codigo: string;
  descricao: string;
  valor: number;
}

export interface ConfirmarArremateRequest {
  liveId: number;
  codigo: string;
  descricao: string;
  valor: number;
  arrematante?: string | null;
  compradorHoraTexto?: string | null;
  matches: MatchDto[];
  googleSheetUrl?: string | null;
  sheetName?: string | null;
}

export interface AuditoriaMatchDto {
  posicao: number;
  username: string;
  textoDigitado: string;
  timestamp: string;
  timestampFmt: string;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class LiveTrackerService {
  private hubConnection?: signalR.HubConnection;
  private readonly apiUrl = environment.apiUrl;
  private readonly hubUrl = environment.apiUrl.replace(/\/api$/, '') + '/hubs/live-chat';

  // Streams reativos de eventos SignalR
  public novoComentario$ = new Subject<ComentarioLiveDto>();
  public statusRastreio$ = new BehaviorSubject<EstadoRastreioDto | null>(null);
  public matchDetectado$ = new Subject<MatchDto>();
  public matchesAtualizados$ = new Subject<MatchDto[]>();
  public pecaFinalizada$ = new Subject<any>();
  public conexaoStatus$ = new BehaviorSubject<boolean>(false);

  private audioCtx?: AudioContext;

  constructor(private http: HttpClient) {}

  /**
   * Conecta ao Hub SignalR e se inscreve na sala da live
   */
  public async iniciarConexao(liveId: number, liveVideoId?: number): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('EntrarNaLive', liveId.toString());
      if (liveVideoId) {
        await this.hubConnection.invoke('EntrarNaLive', liveVideoId.toString());
      }
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .build();

    // Listeners de eventos
    this.hubConnection.on('NovoComentario', (comentario: ComentarioLiveDto) => {
      this.novoComentario$.next(comentario);
    });

    this.hubConnection.on('StatusRastreio', (estado: EstadoRastreioDto) => {
      this.statusRastreio$.next(estado);
    });

    this.hubConnection.on('MatchDetectado', (match: MatchDto) => {
      this.matchDetectado$.next(match);
      this.tocarSomMatch(match.posicao === 1);
    });

    this.hubConnection.on('MatchesAtualizados', (matches: MatchDto[]) => {
      this.matchesAtualizados$.next(matches);
    });

    this.hubConnection.on('PecaFinalizada', (arremate: any) => {
      this.pecaFinalizada$.next(arremate);
    });

    try {
      await this.hubConnection.start();
      this.conexaoStatus$.next(true);
      await this.hubConnection.invoke('EntrarNaLive', liveId.toString());
      if (liveVideoId) {
        await this.hubConnection.invoke('EntrarNaLive', liveVideoId.toString());
      }
      console.log('✅ SignalR Conectado à Live ' + liveId);
    } catch (err) {
      console.error('❌ Erro ao conectar ao SignalR:', err);
      this.conexaoStatus$.next(false);
    }

    this.hubConnection.onreconnected(async () => {
      this.conexaoStatus$.next(true);
      await this.hubConnection?.invoke('EntrarNaLive', liveId.toString());
    });

    this.hubConnection.onclose(() => {
      this.conexaoStatus$.next(false);
    });
  }

  public async desconectar(liveId: number): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.invoke('SairDaLive', liveId.toString());
        await this.hubConnection.stop();
      } catch {}
      this.conexaoStatus$.next(false);
    }
  }

  // ===== Endpoints REST =====

  public iniciarRastreio(req: IniciarRastreioRequest): Observable<EstadoRastreioDto> {
    return this.http.post<EstadoRastreioDto>(`${this.apiUrl}/LiveTracker/iniciar-rastreio`, req);
  }

  public pararRastreio(liveId: number): Observable<EstadoRastreioDto> {
    return this.http.post<EstadoRastreioDto>(`${this.apiUrl}/LiveTracker/parar-rastreio/${liveId}`, {});
  }

  public removerMatch(liveId: number, index: number): Observable<EstadoRastreioDto> {
    return this.http.delete<EstadoRastreioDto>(`${this.apiUrl}/LiveTracker/remover-match/${liveId}/${index}`);
  }

  public obterEstadoAtual(liveId: number): Observable<EstadoRastreioDto> {
    return this.http.get<EstadoRastreioDto>(`${this.apiUrl}/LiveTracker/estado-atual/${liveId}`);
  }

  public confirmarArremate(req: ConfirmarArremateRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/LiveTracker/confirmar-arremate`, req);
  }

  public obterAuditoria(liveId: number, codigo: string, liveVideoId?: number): Observable<AuditoriaMatchDto[]> {
    const params: any = {};
    if (liveVideoId) params.liveVideoId = liveVideoId;
    return this.http.get<AuditoriaMatchDto[]>(`${this.apiUrl}/LiveTracker/auditoria/${liveId}/${codigo}`, { params });
  }

  public injetarComentarioSSN(req: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/LiveTracker/injetar-comentario-ssn`, req);
  }

  public obterArrematesDaLive(liveId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Arremates/GetArrematesByLiveID?liveID=${liveId}`);
  }

  public sincronizarPlanilhaRetroativo(liveId: number, googleSheetUrl?: string, sheetName?: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/LiveTracker/sincronizar-planilha/${liveId}`, {
      googleSheetUrl,
      sheetName
    });
  }

  public configurarPlanilha(liveId: number, googleSheetUrl: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/LiveTracker/configurar-planilha/${liveId}`, {
      googleSheetUrl
    });
  }

  // ===== Feedback Sonoro (Web Audio API) =====
  public tocarSomMatch(isComprador: boolean): void {
    try {
      if (!this.audioCtx) {
        const AudioContextClass = window.AudioContext || (window as any).webkitAudioContext;
        this.audioCtx = new AudioContextClass();
      }

      if (this.audioCtx.state === 'suspended') {
        this.audioCtx.resume();
      }

      const now = this.audioCtx.currentTime;
      const osc = this.audioCtx.createOscillator();
      const gain = this.audioCtx.createGain();

      osc.connect(gain);
      gain.connect(this.audioCtx.destination);

      if (isComprador) {
        // "Ding" alegre e triunfante para o comprador (880Hz -> 1320Hz)
        osc.type = 'sine';
        osc.frequency.setValueAtTime(880, now);
        osc.frequency.exponentialRampToValueAtTime(1320, now + 0.12);
        gain.gain.setValueAtTime(0.25, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.4);
        osc.start(now);
        osc.stop(now + 0.4);
      } else {
        // "Plop" suave para quem entrou na fila (660Hz)
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(660, now);
        gain.gain.setValueAtTime(0.15, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.25);
        osc.start(now);
        osc.stop(now + 0.25);
      }
    } catch (e) {
      // Audio não disponível ou bloqueado por política de autoplay
    }
  }
}
