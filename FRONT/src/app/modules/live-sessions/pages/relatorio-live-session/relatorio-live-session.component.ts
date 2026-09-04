import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { GridModule, PageService, SortService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastModule } from '@syncfusion/ej2-angular-notifications';
import { LiveSessionService, LiveSession } from '../../services/live-session.service';
import { ComentarioLiveService, ComentarioLive } from '../../services/comentario-live.service';

interface ComentarioRow extends ComentarioLive {
  commentTimestampFmt: string;
}

@Component({
  selector: 'app-relatorio-live-session',
  templateUrl: './relatorio-live-session.component.html',
  styleUrls: ['./relatorio-live-session.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    ButtonModule,
    ToastModule
  ],
  providers: [PageService, SortService]
})
export class RelatorioLiveSessionComponent implements OnInit {
  liveSessionId!: number;
  session: LiveSession | null = null;
  comentarios: ComentarioRow[] = [];
  carregando = false;
  erro = '';

  pageSettings = { pageSize: 20 };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private liveSessionService: LiveSessionService,
    private comentarioLiveService: ComentarioLiveService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.liveSessionId = idParam ? Number(idParam) : NaN;

    if (!this.liveSessionId || Number.isNaN(this.liveSessionId)) {
      this.erro = 'Sessão de live inválida.';
      return;
    }

    this.carregar();
  }

  carregar(): void {
    this.carregando = true;
    this.erro = '';

    this.liveSessionService.buscarPorId(this.liveSessionId).subscribe({
      next: (session) => {
        this.session = session;
      },
      error: () => {
        this.erro = 'Não foi possível carregar os dados da live.';
      }
    });

    this.comentarioLiveService.listarPorLiveSession(this.liveSessionId).subscribe({
      next: (comentarios) => {
        this.comentarios = comentarios.map(c => ({
          ...c,
          commentTimestampFmt: this.formatarData(c.commentTimestamp)
        }));
        this.carregando = false;
      },
      error: () => {
        this.erro = 'Não foi possível carregar os comentários desta live.';
        this.carregando = false;
      }
    });
  }

  voltar(): void {
    this.router.navigate(['/live-sessions']);
  }

  private formatarData(dateStr: string | null): string {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}:${String(d.getSeconds()).padStart(2, '0')}`;
  }
}
