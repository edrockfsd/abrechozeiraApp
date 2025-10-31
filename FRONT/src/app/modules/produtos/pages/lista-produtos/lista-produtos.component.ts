import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { GridModule, PagerModule, FilterService, SortService, PageService } from '@syncfusion/ej2-angular-grids';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ProdutoService } from '../../services/produto.service';
import { Produto, ProdutoStatus } from '../../models/produto';




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
    ButtonModule,
    ToastModule
  ],
  providers: [FilterService, SortService, PageService]
})
export class ListaProdutosComponent implements OnInit {
  @ViewChild('toast') public toast!: ToastComponent;
  produtos: any[] = [];
  categorias = ['Roupas', 'Calçados', 'Acessórios'];
  marcas = ['Nike', 'Adidas', 'Puma', 'Outras'];

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  constructor(
    private produtoService: ProdutoService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.produtoService.listarCompleto().subscribe(
      (produtos) => {
        // Normaliza campos vindos como PascalCase da API
        this.produtos = (produtos as any[]).map(p => ({
          ...p,
          marca: p.marca ?? p.Marca ?? '',
          perfil: p.perfil ?? p.Perfil ?? '',
          statusId: p.statusId ?? p.StatusId ?? p.statusID ?? p.StatusID ?? p.status ?? p.ProdutoStatus?.Id ?? 0
        }));
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
          this.toast.show({
            title: 'Sucesso',
            content: 'Produto excluído com sucesso.',
            cssClass: 'e-toast-success',
            icon: 'e-success toast-icons'
          });
        },
        (erro) => {
          console.error('Erro ao excluir produto:', erro);
          this.toast.show({
            title: 'Erro',
            content: 'Erro ao excluir produto. Tente novamente.',
            cssClass: 'e-toast-danger',
            icon: 'e-error toast-icons'
          });
        }
      );
    }
  }
}
