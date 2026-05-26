import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { GridComponent, GridModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { LiveService } from '../../services/live.service';
import { Live } from '../../models/live';
import { ToastService } from '../../../../services/toast.service';

@Component({
  selector: 'app-lista-lives',
  templateUrl: './lista-lives.component.html',
  styleUrls: ['./lista-lives.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    ButtonModule,
    ToastModule
  ]
})
export class ListaLivesComponent implements OnInit {
  @ViewChild('grid') public grid: GridComponent;
  @ViewChild('toast') private toastObj: ToastComponent;

  public lives: Live[] = [];
  public pageSettings = { pageSize: 10 };
  public toolbar = ['Search'];
  public filterSettings = { type: 'Excel' };

  constructor(
    private liveService: LiveService,
    private router: Router,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.carregarLives();
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

  formatarData(data: string): string {
    if (!data) return '';
    const d = new Date(data);
    return d.toLocaleDateString('pt-BR');
  }
} 