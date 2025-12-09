import { Component, HostListener, OnInit } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AutoCompleteModule, FilteringEventArgs, SelectEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { GridModule, RowSelectEventArgs } from '@syncfusion/ej2-angular-grids';
import { PdvService } from '../../services/pdv.service';
import { ProdutoService } from '../../../produtos/services/produto.service';
import { Observable } from 'rxjs';
import { PaymentPanelComponent } from '../payment-panel/payment-panel.component';
import { ActivatedRoute } from '@angular/router';
import { CaixaService } from '../../../caixa/services/caixa.service';

@Component({
  selector: 'app-pdv-shell',
  templateUrl: './pdv-shell.component.html',
  styleUrls: ['./pdv-shell.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AutoCompleteModule, ButtonModule, GridModule, PaymentPanelComponent]
})
export class PdvShellComponent implements OnInit {
  constructor(
    private fb: FormBuilder,
    private pdv: PdvService,
    private produtoService: ProdutoService,
    private route: ActivatedRoute,
    private caixa: CaixaService
  ) {
    this.searchCtrl = this.fb.control('');
    this.vm$ = this.pdv.state$ as any;
  }

  searchCtrl: FormControl;
  vm$: Observable<{ venda: any, itens: any[], pagamentos: any[] }>;
  showPayments = false;
  selectedItemId: number | null = null;
  suggestions: any[] = [];
  autoFields: any = { value: 'descricao' };

  // Keyboard shortcuts
  @HostListener('window:keydown', ['$event']) onKey(ev: KeyboardEvent) {
    if (ev.key === 'F2') { ev.preventDefault(); this.novaVenda(); }
    if (ev.key === 'F8') { ev.preventDefault(); this.finalizar(); }
    if (ev.key === 'F3') { ev.preventDefault(); this.focusSearch(); }
    if (ev.key === 'F4') { ev.preventDefault(); this.editarQuantidade(); }
    if (ev.key === 'F6') { ev.preventDefault(); this.editarDesconto(); }
    if (ev.key === 'Delete') { ev.preventDefault(); this.removerUltimoItem(); }
    if (ev.key === 'Escape') { ev.preventDefault(); this.cancelar(); }
  }

  ngOnInit(): void { 
    const idParam = (this as any).route?.snapshot?.queryParamMap?.get('id');
    const id = idParam ? Number(idParam) : NaN;
    if (isFinite(id) && id > 0) {
      this.pdv.carregar(id).subscribe();
    }
  }

  novaVenda() {
    const caixaId = this.caixa.currentCaixaId;
    if (!caixaId) {
      window.alert('Abra um caixa antes de iniciar uma venda.');
      return;
    }
    this.pdv.novaVenda(caixaId).subscribe();
  }

  finalizar() {
    // open payment panel
    let vendaId: number | null = null;
    const sub = this.vm$.subscribe(vm => vendaId = vm.venda?.id || null);
    sub.unsubscribe();
    if (!vendaId) {
      this.pdv.novaVenda().subscribe(() => this.showPayments = true);
    } else {
      this.showPayments = true;
    }
  }

  private getSnapshot(): { venda: any|null, itens: any[], pagamentos: any[] } {
    let snap: any = { venda: null, itens: [], pagamentos: [] };
    const sub = this.vm$.subscribe(vm => snap = vm || snap);
    sub.unsubscribe();
    return snap;
  }

  focusSearch() {
    const el = document.querySelector('input.scan-input') as HTMLInputElement | null;
    if (el) { el.focus(); el.select?.(); }
  }

  editarQuantidade() {
    const vm = this.getSnapshot();
    const vendaId = vm?.venda?.id ?? null;
    const itens = (vm?.itens || []) as any[];
    const target = this.selectedItemId ? itens.find(i => i.id === this.selectedItemId) : itens.slice(-1)[0];
    if (!vendaId || !target?.id) return;
    const q = window.prompt('Quantidade', String(target.quantidade ?? 1));
    const qtd = q ? Number(q) : NaN;
    if (!isFinite(qtd) || qtd <= 0) return;
    const payload = { ...target, quantidade: qtd };
    this.pdv.updateItem(vendaId, target.id, payload).subscribe(() => this.pdv.carregar(vendaId).subscribe());
  }

  editarDesconto() {
    const vm = this.getSnapshot();
    const vendaId = vm?.venda?.id ?? null;
    const itens = (vm?.itens || []) as any[];
    const target = this.selectedItemId ? itens.find(i => i.id === this.selectedItemId) : itens.slice(-1)[0];
    if (!vendaId || !target?.id) return;
    const d = window.prompt('Desconto (valor)', String(target.descontoValor ?? 0));
    const desc = d ? Number(d) : 0;
    if (!isFinite(desc) || desc < 0) return;
    const total = (Number(target.precoUnitario||0) * Number(target.quantidade||1)) - desc;
    const payload = { ...target, descontoValor: desc, total };
    this.pdv.updateItem(vendaId, target.id, payload).subscribe(() => this.pdv.carregar(vendaId).subscribe());
  }

  removerUltimoItem() {
    const vm = this.getSnapshot();
    const vendaId = vm?.venda?.id ?? null;
    const itens = (vm?.itens || []) as any[];
    const target = this.selectedItemId ? itens.find(i => i.id === this.selectedItemId) : itens.slice(-1)[0];
    if (!vendaId || !target?.id) return;
    this.pdv.deleteItem(vendaId, target.id).subscribe(() => this.pdv.carregar(vendaId).subscribe());
  }

  // seleção de item na grade (realça para ações rápidas)
  selectItem(it: any) { this.selectedItemId = it?.id ?? null; }

  cancelar() {
    if (this.showPayments) { this.showPayments = false; return; }
    const vm = this.getSnapshot();
    const vendaId = vm?.venda?.id ?? null;
    if (!vendaId) return;
    this.pdv.cancelar(vendaId).subscribe(() => this.pdv.carregar(vendaId).subscribe());
  }

  

  // Utilitário para pegar item selecionado (ou o último)
  getSelectedItemFromVm(vm: any): any | null {
    const itens = (vm?.itens || []) as any[];
    if (!itens.length) { return null; }
    if (this.selectedItemId) {
      const hit = itens.find(i => i.id === this.selectedItemId);
      if (hit) return hit;
    }
    return itens[itens.length - 1];
  }

  // item-level actions used by template
  editarQuantidadeItem(item: any) {
    const vm = this.getSnapshot();
    const vendaId = vm?.venda?.id ?? null;
    const target = item;
    if (!vendaId || !target?.id) return;
    const q = window.prompt('Quantidade', String(target.quantidade ?? 1));
    const qtd = q ? Number(q) : NaN;
    if (!isFinite(qtd) || qtd <= 0) return;
    const payload = { ...target, quantidade: qtd };
    this.pdv.updateItem(vendaId, target.id, payload).subscribe(() => this.pdv.carregar(vendaId).subscribe());
  }

  editarDescontoItem(item: any) {
    const vm = this.getSnapshot();
    const vendaId = vm?.venda?.id ?? null;
    const target = item;
    if (!vendaId || !target?.id) return;
    const d = window.prompt('Desconto (valor)', String(target.descontoValor ?? 0));
    const desc = d ? Number(d) : 0;
    if (!isFinite(desc) || desc < 0) return;
    const total = (Number(target.precoUnitario||0) * Number(target.quantidade||1)) - desc;
    const payload = { ...target, descontoValor: desc, total };
    this.pdv.updateItem(vendaId, target.id, payload).subscribe(() => this.pdv.carregar(vendaId).subscribe());
  }

  removerItem(item: any) {
    const vm = this.getSnapshot();
    const vendaId = vm?.venda?.id ?? null;
    const target = item;
    if (!vendaId || !target?.id) return;
    this.pdv.deleteItem(vendaId, target.id).subscribe(() => this.pdv.carregar(vendaId).subscribe());
  }

  onSearchEnter() {
    const code = (this.searchCtrl.value || '').toString().trim();
    if (!code) { return; }

    const isNumeric = /^\d+$/.test(code);
    if (isNumeric) {
      let vendaId: number | null = null;
      const sub0 = this.vm$.subscribe(vm => vendaId = vm.venda?.id || null);
      sub0.unsubscribe();
      const ensureVenda = (cb: (vid: number) => void) => {
        if (!vendaId) {
          const caixaId = this.caixa.currentCaixaId;
          if (!caixaId) { window.alert('Abra um caixa antes de registrar itens.'); return; }
          this.pdv.novaVenda(caixaId).subscribe(id => cb(id));
        } else {
          cb(vendaId);
        }
      };
      this.produtoService.buscarPorCodigoEstoque(code).subscribe({
        next: (prod: any) => {
          if (!prod || !prod.id || prod.id <= 0) { window.alert('Produto nao encontrado para o codigo informado.'); return; }
          ensureVenda((vid) => {
            const item = this.buildItemFromProduto(prod);
            this.pdv.addItem(vid, item).subscribe(() => { this.pdv.carregar(vid).subscribe(); this.searchCtrl.setValue(''); });
          });
        },
        error: () => { window.alert('Produto nao encontrado para o codigo informado.'); }
      });
      return;
    }

    // text search; if Enter pressed, take first result
    this.produtoService.search(code).subscribe({
      next: (list: any[]) => {
        if (!list || list.length === 0) { window.alert('Nenhum produto encontrado.'); return; }
        const prod = list[0];
        let vendaId: number | null = null;
        const sub1 = this.vm$.subscribe(vm => vendaId = vm.venda?.id || null);
        sub1.unsubscribe();
        const ensureVenda = (cb: (vid: number) => void) => {
          if (!vendaId) {
            const caixaId = this.caixa.currentCaixaId;
            if (!caixaId) { window.alert('Abra um caixa antes de registrar itens.'); return; }
            this.pdv.novaVenda(caixaId).subscribe(id => cb(id));
          } else {
            cb(vendaId);
          }
        };
        ensureVenda((vid) => {
          const item = this.buildItemFromProduto(prod);
          this.pdv.addItem(vid, item).subscribe(() => { this.pdv.carregar(vid).subscribe(); this.searchCtrl.setValue(''); });
        });
      },
      error: () => { window.alert('Erro na busca.'); }
    });
  }

  private buildItemFromProduto(prod: any) {
    const preco = Number((prod?.precoVenda ?? prod?.PrecoVenda ?? 0));
    const cod = (prod?.codigoEstoque ?? prod?.CodigoEstoque);
    return {
      produtoId: prod?.id ?? prod?.Id ?? null,
      descricaoItem: prod?.descricao ?? prod?.Descricao ?? '',
      quantidade: 1,
      precoUnitario: preco,
      descontoValor: 0,
      total: preco,
      codigoEstoque: (cod !== undefined && cod !== null) ? String(cod) : undefined
    } as any;
  }

  selectSuggestion(prod: any) {
    let vendaId: number | null = null;
    const sub = this.vm$.subscribe(vm => vendaId = vm.venda?.id || null);
    sub.unsubscribe();
    const doAdd = (vid: number) => {
      const item = this.buildItemFromProduto(prod);
      this.pdv.addItem(vid, item).subscribe(() => {
        this.pdv.carregar(vid).subscribe();
        this.searchCtrl.setValue('');
      });
    };
    if (!vendaId) this.pdv.novaVenda().subscribe(id => doAdd(id)); else doAdd(vendaId);
  }

  // Syncfusion AutoComplete handlers
  onFiltering(e: FilteringEventArgs) {
    const text = (e.text || '').trim();
    e.preventDefaultAction = true;
    if (!text) { e.updateData([]); return; }
    this.produtoService.search(text).subscribe({
      next: (list: any[]) => { e.updateData(list || []); },
      error: () => { e.updateData([]); }
    });
  }

  onSelect(e: SelectEventArgs) {
    const data: any = e.itemData as any;
    if (!data) return;
    this.selectSuggestion(data);
  }

  // Finalização do painel de pagamentos
  onFinalized() {
    this.showPayments = false;
    const caixaId = this.caixa.currentCaixaId;
    if (!caixaId) { return; }
    this.pdv.novaVenda(caixaId).subscribe();
  }

  // Grid handlers
  onItemRowSelected(e: RowSelectEventArgs) {
    const data: any = (e?.data as any) || null;
    this.selectedItemId = data?.id ?? null;
  }

  codeAccessor(field: string, data: any): string {
    return String(data?.codigoEstoque ?? data?.produtoId ?? '-');
  }

  
}
