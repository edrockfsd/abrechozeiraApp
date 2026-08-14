import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { PdvService } from '../../services/pdv.service';
import { NfceService, Nfce } from '../../services/nfce.service';

@Component({
  selector: 'app-sales-receipt',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sales-receipt.component.html',
  styleUrls: ['./sales-receipt.component.scss']
})
export class SalesReceiptComponent implements OnInit {
  venda: any;
  itens: any[] = [];
  pagamentos: any[] = [];
  nfce: Nfce | null = null;
  emitindoNfce = false;
  mensagemNfce: string | null = null;
  erroNfce: string | null = null;

  modoDanfe = true;

  constructor(
    private route: ActivatedRoute,
    private pdv: PdvService,
    private nfceService: NfceService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (isFinite(id) && id > 0) {
      this.pdv.getVendaRaw(id).subscribe(data => {
        this.venda = data.venda;
        this.itens = data.itens;
        this.pagamentos = data.pagamentos;
        this.buscarNfce(id);
      });
    }
  }

  buscarNfce(vendaId: number): void {
    this.nfceService.listar({ limite: 100 }).subscribe(lista => {
      this.nfce = lista.find(n => n.vendaPdvId === vendaId) || null;
    });
  }

  emitirNfce(): void {
    if (!this.venda?.id) return;
    this.emitindoNfce = true;
    this.mensagemNfce = null;
    this.erroNfce = null;

    this.nfceService.emitirVendaPdv(this.venda.id).subscribe({
      next: (res) => {
        this.emitindoNfce = false;
        this.nfce = res;
        this.modoDanfe = true;
        this.mensagemNfce = `NFC-e Nº ${res.numero} emitida e autorizada com sucesso! Chave: ${res.chaveAcesso}`;
      },
      error: (err) => {
        this.emitindoNfce = false;
        this.erroNfce = err.error?.erro || 'Erro ao emitir NFC-e';
      }
    });
  }

  formatarChave(chave?: string): string {
    if (!chave) return '';
    return chave.replace(/(.{4})/g, '$1 ').trim();
  }

  getQrCodeUrl(chave?: string): string {
    if (!chave) return '';
    const urlSefaz = `http://www.fazenda.pr.gov.br/nfce/consulta?p=${chave}`;
    return `https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=${encodeURIComponent(urlSefaz)}`;
  }

  alternarModo(danfe: boolean): void {
    this.modoDanfe = danfe;
  }

  imprimir(): void {
    window.print();
  }
}

