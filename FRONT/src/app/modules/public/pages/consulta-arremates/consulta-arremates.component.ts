import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClientPortalService } from '../../services/client-portal.service';

@Component({
  selector: 'app-consulta-arremates',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './consulta-arremates.component.html',
  styleUrls: ['./consulta-arremates.component.scss']
})
export class ConsultaArrematesComponent implements OnInit {
  lives: any[] = [];
  selectedLiveId: number | null = null;
  nickInstagram: string = '';
  
  resultados: any = null;
  loading: boolean = false;
  erro: string = '';

  constructor(private portalService: ClientPortalService) {}

  ngOnInit(): void {
    this.portalService.getLives().subscribe({
      next: (data) => {
        // Filtrar apenas lives que tem planilha
        this.lives = data.filter(l => l.hasSheet);
      }
    });
  }

  consultar(): void {
    if (!this.selectedLiveId || !this.nickInstagram) {
      this.erro = 'Preencha todos os campos.';
      return;
    }
    this.erro = '';
    this.loading = true;
    this.resultados = null;

    this.portalService.getMeusArremates(this.selectedLiveId, this.nickInstagram).subscribe({
      next: (res) => {
        this.loading = false;
        this.resultados = res;
      },
      error: (err) => {
        this.loading = false;
        this.erro = 'Não foi possível buscar seus arremates. Tente novamente.';
      }
    });
  }
}
