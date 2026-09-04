import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { GridModule, PageService, SortService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastModule } from '@syncfusion/ej2-angular-notifications';
import { LiveSessionService, LiveSession } from '../../services/live-session.service';

interface LiveSessionRow extends LiveSession {
  startedAtFmt: string;
  primeiroComentarioFmt: string;
}

@Component({
  selector: 'app-lista-live-sessions',
  templateUrl: './lista-live-sessions.component.html',
  styleUrls: ['./lista-live-sessions.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    GridModule,
    ButtonModule,
    ToastModule
  ],
  providers: [PageService, SortService]
})
export class ListaLiveSessionsComponent implements OnInit {
  sessions: LiveSessionRow[] = [];
  carregando = false;

  // Cards de resumo: calculados a partir dos dados reais carregados da API
  // (a versão anterior tinha esses números fixos no HTML).
  totalSessions = 0;
  totalComentarios = 0;

  pageSettings = { pageSize: 15 };

  constructor(
    private liveSessionService: LiveSessionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando = true;
    this.liveSessionService.listar().subscribe({
      next: (sessions) => {
        this.sessions = sessions.map(s => ({
          ...s,
          startedAtFmt: this.formatarData(s.startedAt),
          primeiroComentarioFmt: this.formatarData(s.primeiroComentarioEm)
        }));
        this.totalSessions = sessions.length;
        this.totalComentarios = sessions.reduce((acc, s) => acc + (s.totalComentarios || 0), 0);
        this.carregando = false;
      },
      error: (err) => {
        console.error('Erro ao carregar sessões de live:', err);
        this.carregando = false;
      }
    });
  }

  abrirRelatorio(session: LiveSession): void {
    this.router.navigate(['/live-sessions', session.id, 'relatorio']);
  }

  onRowSelected(args: any): void {
    this.abrirRelatorio(args.data as LiveSession);
  }

  private formatarData(dateStr: string | null): string {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
  }
}
