import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { PdvService } from '../../services/pdv.service';

@Component({
  selector: 'app-sales-receipt',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sales-receipt.component.html',
  styleUrls: ['./sales-receipt.component.scss']
})
export class SalesReceiptComponent implements OnInit {
  venda: any; itens: any[] = []; pagamentos: any[] = [];
  constructor(private route: ActivatedRoute, private pdv: PdvService) {}
  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (isFinite(id) && id>0) {
      this.pdv.getVendaRaw(id).subscribe(data => { this.venda = data.venda; this.itens = data.itens; this.pagamentos = data.pagamentos; setTimeout(()=>window.print(), 300); });
    }
  }
}

