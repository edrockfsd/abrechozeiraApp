import { Component, HostListener, OnInit } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PdvService } from '../../services/pdv.service';
import { ProdutoService } from '../../../produtos/services/produto.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-pdv-shell',
  templateUrl: './pdv-shell.component.html',
  styleUrls: ['./pdv-shell.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})

export class PdvShellComponent implements OnInit {
  constructor(private fb: FormBuilder, private pdv: PdvService, private produtoService: ProdutoService) {
    this.searchCtrl = this.fb.control('');
    this.vm$ = this.pdv.state$ as any;
  }

  searchCtrl: FormControl;
  vm$: Observable<{ venda: any, itens: any[], pagamentos: any[] }>;

  // Atalhos mínimos
  @HostListener('window:keydown', ['$event']) onKey(ev: KeyboardEvent) {
    if (ev.key === 'F2') { ev.preventDefault(); this.novaVenda(); }
    if (ev.key === 'F8') { ev.preventDefault(); this.finalizar(); }
  }

  novaVenda() {
    this.pdv.novaVenda().subscribe();
  }

  finalizar() {
    // TODO: abrir painel de pagamentos
  }

  ngOnInit(): void {
    // opcional: abrir venda automaticamente
  }

  onSearchEnter() {
    const code = (this.searchCtrl.value || '').toString().trim();
    if (!code) { return; }

    // Recupera venda atual do stream
    let vendaId: number | null = null;
    const sub = this.vm$.subscribe(vm => vendaId = vm.venda?.id || null);
    sub.unsubscribe();

    const addItemToVenda = (vid: number) => {
      this.produtoService.buscarPorCodigoEstoque(code).subscribe(prod => {
        const hasId = (prod as any)?.id && (prod as any).id > 0;
        const descricao = hasId ? (prod as any).descricao : code;
        const preco = hasId ? Number((prod as any).precoVenda || 0) : 0;
        const item = {
          produtoId: hasId ? (prod as any).id : null,
          descricaoItem: descricao,
          quantidade: 1,
          precoUnitario: preco,
          descontoValor: 0,
          total: preco,
          codigoEstoque: hasId ? undefined : code
        } as any;
        this.pdv.addItem(vid, item).subscribe(() => {
          this.pdv.carregar(vid).subscribe();
          this.searchCtrl.setValue('');
        });
      });
    };

    if (!vendaId) {
      this.pdv.novaVenda().subscribe(id => addItemToVenda(id));
    } else {
      addItemToVenda(vendaId);
    }
  }
}

