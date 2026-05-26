import { Component, EventEmitter, OnDestroy, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { PdvService, PdvConfig, VendaPdvPagamento } from '../../services/pdv.service';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { NumericTextBoxModule, TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { GridModule } from '@syncfusion/ej2-angular-grids';

@Component({
  selector: 'app-payment-panel',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DropDownListModule, NumericTextBoxModule, TextBoxModule, ButtonModule, GridModule],
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
  @Output() finalized = new EventEmitter<void>();

  form: FormGroup;
  config: PdvConfig | null = null;
  vendaId: number | null = null;
  total: number = 0;
  pagamentos: VendaPdvPagamento[] = [] as any;
  sumPagos = 0;
  restante = 0;
  troco = 0;
  formasFields = { text: 'descricao', value: 'id' } as any;
  condicaoFields = { text: 'descricao', value: 'id' } as any;
  finalizedId: number | null = null;

  private sub?: Subscription;

  ngOnInit(): void {
    // Load config once
    this.pdv.getConfig().subscribe(cfg => this.config = cfg);

    // Subscribe to PDV state for venda/pagamentos/total
    this.sub = this.pdv.state$.subscribe(vm => {
      this.vendaId = vm.venda?.id ?? null;
      this.total = Number(vm.venda?.valorLiquido ?? 0);
      this.pagamentos = (vm.pagamentos || []) as any;
       this.finalizedId = vm.venda?.status === 'Finalizada' ? (vm.venda.id ?? null) : null;
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
    const valorCtrl = this.form?.get('valor');
    if (valorCtrl && this.restante > 0) {
      valorCtrl.setValue(this.restante, { emitEvent: false });
    }
  }

  getFormaDescricao(id: number | null | undefined): string {
    if (!id || !this.config?.formasPagamento) return '';
    const f = this.config.formasPagamento.find(x => x.id === id);
    return f?.descricao || '';
  }

  addPagamento() {
    if (this.finalizedId) return;
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
    // Se ainda falta pagar e o formulário tem um valor/forma, tenta cobrir automaticamente
    if (this.sumPagos < this.total) {
      const restante = this.total - this.sumPagos;
      const vForm = Number(this.form.value?.valor || 0);
      const forma = this.form.value?.formaPagamentoId || null;
      if (restante > 0 && forma && vForm > 0) {
        const payload = {
          formaPagamentoId: forma,
          condicaoPagamentoId: this.form.value?.condicaoPagamentoId || null,
          valor: vForm,
          observacao: this.form.value?.observacao || null
        } as Partial<VendaPdvPagamento>;
        this.pdv.addPagamento(this.vendaId, payload).subscribe(() => {
          this.pdv.carregar(this.vendaId!).subscribe(() => {
            // Recalcula e tenta finalizar novamente
            this.finalizarVenda();
          });
        });
        return;
      }
      return; // sem cobertura automática
    }
    const currentId = this.vendaId;
    this.pdv.finalizar(currentId).subscribe(() => {
      this.pdv.carregar(currentId!).subscribe(() => {
        this.finalizedId = currentId!;
      });
    });
  }

  // Grid helpers
  formaAccessor(field: string, data: any): string {
    return this.getFormaDescricao(data?.formaPagamentoId) || String(data?.formaPagamentoId ?? '');
  }

  printCupom() {
    if (!this.finalizedId) return;
    window.open(`/pdv/vendas/${this.finalizedId}/cupom`, '_blank');
  }

  confirmarNovaVenda() {
    this.finalized.emit();
    this.close.emit();
  }
}
