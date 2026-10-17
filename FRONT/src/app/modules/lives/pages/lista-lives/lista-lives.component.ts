import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription, catchError, concatMap, from, map, of } from 'rxjs';
import { Router } from '@angular/router';
import { GridComponent, GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { LiveService } from '../../services/live.service';
import { Live } from '../../models/live';
import { ToastService } from '../../../../services/toast.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';

type StatusAviso = 'pendente' | 'enviando' | 'ok' | 'erro';

interface AvisoLive {
  link: string;
  carregando: boolean;
  itens: { id: number; nome: string; telefone: string; status: StatusAviso; detalhe?: string }[];
  enviando: boolean;
  concluido: boolean;
  interrompido: boolean;
}

@Component({
  selector: 'app-lista-lives',
  templateUrl: './lista-lives.component.html',
  styleUrls: ['./lista-lives.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    ButtonModule,
    ToastModule,
    FormsModule
  ]
})
export class ListaLivesComponent implements OnInit {
  @ViewChild('grid') public grid: GridComponent;
  @ViewChild('toast') private toastObj: ToastComponent;

  public lives: Live[] = [];
  public liveAtiva: any = null;
  public pageSettings = { pageSize: 10 };
  public toolbar = ['Search'];
  public filterSettings = { type: 'Excel' };

  constructor(
    private liveService: LiveService,
    private router: Router,
    private toastService: ToastService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.carregarLives();
    this.verificarLiveAtiva();
  }

  private verificarLiveAtiva(): void {
    this.http.get<any>(`${environment.apiUrl}/LiveTracker/live-ativa`).subscribe({
      next: (res) => {
        if (res && res.isLive) {
          this.liveAtiva = res;
        } else {
          this.liveAtiva = null;
        }
      },
      error: () => {
        this.liveAtiva = null;
      }
    });
  }

  ngAfterViewInit(): void {
    if (this.toastObj) {
      this.toastService.setToastComponent(this.toastObj);
    }
  }

  private carregarLives(): void {
    this.liveService.listar().subscribe(
      (lives) => {
        this.lives = lives;
      },
      (erro) => {
        console.error('Erro ao carregar lives:', erro);
        this.toastService.showError('Erro ao carregar lives. Por favor, tente novamente.');
      }
    );
  }

  onNovaLive(): void {
    this.router.navigate(['/lives/novo']);
  }

  onOperarLive(live: Live): void {
    this.router.navigate(['/lives', live.id, 'gestao']);
  }

  onOperarLiveAtiva(): void {
    if (!this.liveAtiva || !this.liveAtiva.liveId) return;
    this.router.navigate(['/lives', this.liveAtiva.liveId, 'gestao']);
  }

  onEditar(live: Live): void {
    this.router.navigate(['/lives/editar', live.id]);
  }

  onExcluir(live: Live): void {
    if (confirm('Tem certeza que deseja excluir esta live?')) {
      this.liveService.excluir(live.id).subscribe(
        () => {
          this.toastService.showSuccess('Live excluída com sucesso.');
          this.carregarLives();
        },
        (erro) => {
          console.error('Erro ao excluir live:', erro);
          this.toastService.showError('Erro ao excluir live. Por favor, tente novamente.');
        }
      );
    }
  }

  onToggleStatus(live: Live): void {
    const proximoStatus = live.ativo === false;
    const acao = proximoStatus ? 'ativar' : 'desativar';
    this.liveService.toggleStatus(live.id).subscribe({
      next: (res) => {
        live.ativo = res.ativo;
        this.toastService.showSuccess(`Live "${live.titulo}" ${res.ativo ? 'ativada' : 'desativada'} com sucesso!`);
        if (this.grid) {
          this.grid.refresh();
        }
      },
      error: (erro) => {
        console.error('Erro ao alternar status da live:', erro);
        this.toastService.showError(`Erro ao ${acao} a live. Por favor, tente novamente.`);
      }
    });
  }

  // ─── Aviso de live por WhatsApp ────────────────────────────────────────────

  public avisoLive: AvisoLive | null = null;
  private envioAvisoSub?: Subscription;

  abrirAvisoLive(): void {
    this.avisoLive = { link: '', carregando: true, itens: [], enviando: false, concluido: false, interrompido: false };
    this.liveService.destinatariosAvisoLive().subscribe({
      next: (lista) => {
        if (!this.avisoLive) return;
        this.avisoLive.itens = lista.map(c => ({ ...c, status: 'pendente' as StatusAviso }));
        this.avisoLive.carregando = false;
      },
      error: () => {
        this.avisoLive = null;
        this.toastService.showError('Não foi possível carregar as clientes com telefone.');
      }
    });
  }

  linkInstagramValido(link: string): boolean {
    return /(^|\/\/|\.)instagram\.com\/\S+/i.test((link || '').trim());
  }

  progressoAviso(a: AvisoLive): { feitos: number; ok: number; falhas: number; pct: number } {
    const feitos = a.itens.filter(i => i.status === 'ok' || i.status === 'erro').length;
    const ok = a.itens.filter(i => i.status === 'ok').length;
    return { feitos, ok, falhas: feitos - ok, pct: a.itens.length ? Math.round((feitos / a.itens.length) * 100) : 0 };
  }

  confirmarAvisoLive(): void {
    const a = this.avisoLive;
    if (!a || a.enviando || a.concluido || !this.linkInstagramValido(a.link) || a.itens.length === 0) return;
    if (!confirm(`Enviar o aviso de live por WhatsApp para ${a.itens.length} cliente(s)?\n\nCada mensagem de marketing é cobrada pela Meta.`)) return;

    a.enviando = true;
    const link = a.link.trim();
    this.envioAvisoSub = from(a.itens).pipe(
      concatMap(item => {
        item.status = 'enviando';
        return this.liveService.enviarAvisoLive(item.id, link).pipe(
          map(res => {
            item.status = res.enviado ? 'ok' : 'erro';
            item.detalhe = res.enviado ? 'Enviado' : (res.erro || 'Falha ao enviar');
          }),
          catchError(err => {
            item.status = 'erro';
            item.detalhe = err.error?.message || 'Falha ao enviar';
            return of(null);
          })
        );
      })
    ).subscribe({ complete: () => this.finalizarAvisoLive(false) });
  }

  pararAvisoLive(): void {
    if (!this.avisoLive?.enviando) return;
    this.envioAvisoSub?.unsubscribe();
    this.avisoLive.itens.filter(i => i.status === 'enviando').forEach(i => { i.status = 'pendente'; i.detalhe = undefined; });
    this.finalizarAvisoLive(true);
  }

  private finalizarAvisoLive(interrompido: boolean): void {
    if (!this.avisoLive) return;
    this.avisoLive.enviando = false;
    this.avisoLive.concluido = true;
    this.avisoLive.interrompido = interrompido;
  }

  fecharAvisoLive(): void {
    if (this.avisoLive?.enviando) return;
    this.avisoLive = null;
  }

  formatarData(data: string): string {
    if (!data) return '';
    const d = new Date(data);
    return d.toLocaleDateString('pt-BR');
  }

  onSyncSheet(live: Live): void {
    if (!live.googleSheetUrl) {
      this.toastService.showError('Esta live não tem uma URL de planilha configurada. Edite a live e preencha o campo.');
      return;
    }
    this.toastService.showSuccess('Sincronizando... Isso pode demorar alguns segundos.');
    this.liveService.sincronizarPlanilha(live.id).subscribe({
      next: (res) => {
        this.toastService.showSuccess(res.message);
      },
      error: (err) => {
        console.error('Erro na sincronização:', err);
        this.toastService.showError('Falha ao sincronizar planilha. Verifique a URL e a estrutura.');
      }
    });
  }
} 