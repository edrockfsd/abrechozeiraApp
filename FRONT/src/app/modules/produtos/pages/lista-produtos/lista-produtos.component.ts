import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { GridModule, PagerModule, FilterService, SortService, PageService } from '@syncfusion/ej2-angular-grids';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ProdutoService } from '../../services/produto.service';
import { Produto, ProdutoStatus } from '../../models/produto';

import { L10n, loadCldr, setCulture, setCurrencyCode } from '@syncfusion/ej2-base';
import * as cagregorian from "../../../../shared/ca-gregorian.json";
import * as currencies from "../../../../shared/currencies.json";
import * as numbers from "../../../../shared/numbers.json";
import * as timeZoneNames from "../../../../shared/timeZoneNames.json";
import * as numberingSystems from "../../../../shared/numberingSystmes.json"
setCulture('pt');
setCurrencyCode('BRL');
loadCldr(numberingSystems['default'],cagregorian['default'],currencies['default'], numbers['default'], timeZoneNames['default']); 

@Component({
  selector: 'app-lista-produtos',
  templateUrl: './lista-produtos.component.html',
  styleUrls: ['./lista-produtos.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    PagerModule,
    DropDownListModule,
    ButtonModule
  ],
  providers: [FilterService, SortService, PageService]
})
export class ListaProdutosComponent implements OnInit {
  produtos: Produto[] = [];
  categorias = ['Roupas', 'Calçados', 'Acessórios'];
  marcas = ['Nike', 'Adidas', 'Puma', 'Outras'];

  constructor(
    private produtoService: ProdutoService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.produtoService.listar().subscribe(
      (produtos) => {
        this.produtos = produtos;
      },
      (erro) => {
        console.error('Erro ao carregar produtos:', erro);
      }
    );
  }

  getStatusText(statusId: number): string {
    switch (statusId) {
      case ProdutoStatus.Ativo:
        return 'Ativo';
      case ProdutoStatus.Inativo:
        return 'Inativo';
      case ProdutoStatus.Excluido:
        return 'Excluído';
      default:
        return 'Desconhecido';
    }
  }

  getStatusClass(statusId: number): string {
    switch (statusId) {
      case ProdutoStatus.Ativo:
        return 'status-ativo';
      case ProdutoStatus.Inativo:
        return 'status-inativo';
      case ProdutoStatus.Excluido:
        return 'status-excluido';
      default:
        return '';
    }
  }

  onNovoProduto(): void {
    this.router.navigate(['/produtos/novo']);
  }

  onEditar(produto: Produto): void {
    this.router.navigate(['/produtos/editar', produto.id]);
  }

  onExcluir(produto: Produto): void {
    if (confirm('Tem certeza que deseja excluir este produto?')) {
      this.produtoService.excluir(produto.id).subscribe(
        () => {
          this.carregarProdutos();
        },
        (erro) => {
          console.error('Erro ao excluir produto:', erro);
        }
      );
    }
  }
}
