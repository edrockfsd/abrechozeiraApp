import { Component, EventEmitter, OnDestroy, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { PdvService, PdvConfig, VendaPdvPagamento } from '../../services/pdv.service';

@Component({
  selector: 'app-payment-panel',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './payment-panel.component.html',
  styleUrls: ['./payment-panel.component.scss']
})
export class PaymentPanelComponent implements OnInit, OnDestroy {
  constructor(private fb: FormBuilder, private pdv: PdvService) {
    this.form = this.fb.group({
      formaPagamentoId: [null, Validators.required],
      condicaoPagamentoId: [null],
      valor: [null, [Validators.required, Validators.min(0.01)]],
      observacao: ['']
    });
  }

  @Output() close = new EventEmitter<void>();

  form: FormGroup;
  config: PdvConfig | null = null;
  vendaId: number | null = null;
  total: number = 0;
  pagamentos: VendaPdvPagamento[] = [] as any;
  sumPagos = 0;
  restante = 0;
  troco = 0;

  private sub?: Subscription;

  ngOnInit(): void {
    // Load config once
    this.pdv.getConfig().subscribe(cfg => this.config = cfg);

    // Subscribe to PDV state for venda/pagamentos/total
    this.sub = this.pdv.state$.subscribe(vm => {
      this.vendaId = vm.venda?.id ?? null;
      this.total = Number(vm.venda?.valorLiquido ?? 0);
      this.pagamentos = (vm.pagamentos || []) as any;
      this.recalc();
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private recalc() {
    this.sumPagos = (this.pagamentos || []).reduce((acc, p) => acc + Number(p.valor || 0), 0);
    const diff = this.total - this.sumPagos;
    this.restante = diff > 0 ? diff : 0;
    this.troco = diff < 0 ? Math.abs(diff) : 0;
  }

  addPagamento() {
    if (!this.vendaId || this.form.invalid) return;
    const payload = {
      formaPagamentoId: this.form.value.formaPagamentoId,
      condicaoPagamentoId: this.form.value.condicaoPagamentoId || null,
      valor: Number(this.form.value.valor || 0),
      observacao: this.form.value.observacao || null
    } as Partial<VendaPdvPagamento>;

    this.pdv.addPagamento(this.vendaId, payload).subscribe(() => {
      this.pdv.carregar(this.vendaId!).subscribe();
      this.form.reset();
    });
  }

  remover(pg: VendaPdvPagamento) {
    if (!this.vendaId || !pg?.id) return;
    this.pdv.deletePagamento(this.vendaId, pg.id).subscribe(() => {
      this.pdv.carregar(this.vendaId!).subscribe();
    });
  }

  finalizarVenda() {
    if (!this.vendaId) return;
    // Permite finalizar quando total pago >= total devido
    if (this.sumPagos < this.total) return;
    this.pdv.finalizar(this.vendaId).subscribe(() => {
      this.close.emit();
      // Recarrega estado finalizado
      this.pdv.carregar(this.vendaId!).subscribe();
    });
  }
}

