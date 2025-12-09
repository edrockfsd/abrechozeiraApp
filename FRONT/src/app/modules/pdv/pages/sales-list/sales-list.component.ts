import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { DateRangePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { FormsModule } from '@angular/forms';
import { PdvService } from '../../services/pdv.service';

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, GridModule, DateRangePickerModule, DropDownListModule, ButtonModule],
  templateUrl: './sales-list.component.html',
  styleUrls: ['./sales-list.component.scss']
})
export class SalesListComponent implements OnInit {
  constructor(private pdv: PdvService) {}

  vendas: any[] = [];
  loading = false;
  range: any;
  status: string | null = null;
  statusOptions = [
    { text: 'Todas', value: null },
    { text: 'Aberta', value: 'Aberta' },
    { text: 'Finalizada', value: 'Finalizada' },
    { text: 'Cancelada', value: 'Cancelada' }
  ];

  ngOnInit(): void { this.load(); }

  private toIso(d: Date): string { return new Date(d).toISOString(); }

  load() {
    this.loading = true;
    let startIso: string|undefined;
    let endIso: string|undefined;
    if (this.range?.startDate) startIso = this.toIso(this.range.startDate as Date);
    if (this.range?.endDate) endIso = this.toIso(this.range.endDate as Date);
    this.pdv.listVendas(100, this.status || undefined, startIso, endIso).subscribe({
      next: (list) => {
        const base = list || [];
        this.vendas = !this.status ? base.filter(v => v.status !== 'Cancelada') : base;
        this.loading = false;
      },
      error: () => { this.vendas = []; this.loading = false; }
    });
  }

  cancelarVenda(venda: any) {
    if (!venda?.id) { return; }
    const ok = window.confirm(`Cancelar venda #${venda.id}?`);
    if (!ok) return;
    this.pdv.cancelar(venda.id).subscribe(() => this.load());
  }
}
