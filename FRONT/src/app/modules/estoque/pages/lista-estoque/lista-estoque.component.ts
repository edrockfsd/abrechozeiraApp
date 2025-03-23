import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import {
  GridComponent, GridModule, EditService, ToolbarService,
  PageService, FilterService, SortService, CommandColumnService
} from '@syncfusion/ej2-angular-grids';
import { ToolbarItems, CommandModel } from '@syncfusion/ej2-grids';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { EstoqueService } from '../../services/estoque.service';
import { ProdutoService } from '../../../produtos/services/produto.service';
import { Estoque, EstoqueCreate } from '../../models/estoque';
import { NumericTextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { ToastService } from '../../../../services/toast.service';

@Component({
  selector: 'app-lista-estoque',
  templateUrl: './lista-estoque.component.html',
  styleUrls: ['./lista-estoque.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    GridModule,
    ToastModule,
    NumericTextBoxModule,
    DropDownListModule,
    DatePickerModule
  ],
  providers: [
    EditService,
    ToolbarService,
    PageService,
    FilterService,
    SortService,
    CommandColumnService
  ]
})
export class ListaEstoqueComponent implements OnInit {
  @ViewChild('grid') public grid: GridComponent;
  @ViewChild('toast') private toastObj: ToastComponent;

  public data: Estoque[] = [];
  public editSettings: Object;
  public toolbar: ToolbarItems[];
  public commands: CommandModel[];
  public pageSettings: Object;
  public filterSettings: Object;
  public isEdicao: boolean = false;

  public quantidadeParams = {
    params: {
      min: 0,
      format: 'N0',
      validateDecimalOnType: true,
      decimals: 0,
      step: 1
    }
  };

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  constructor(
    private estoqueService: EstoqueService,
    private produtoService: ProdutoService,
    private toastService: ToastService
  ) {
    this.editSettings = {
      allowEditing: true,
      allowAdding: false,
      allowDeleting: false,
      mode: 'Dialog',
      showDeleteConfirmDialog: false
    };
    this.toolbar = ['Search'];
    this.commands = [
      {
        type: 'Edit',
        buttonOption: { cssClass: 'e-flat', iconCss: 'e-edit e-icons' }
      }
    ];
    this.pageSettings = { pageSize: 10 };
    this.filterSettings = { type: 'Menu' };
  }

  ngOnInit(): void {
    this.carregarDados();
  }

  private carregarDados(): void {
    console.log('Iniciando carregamento dos dados do estoque...');
    this.estoqueService.listar().subscribe(
      data => {
        console.log('Dados do estoque recebidos:', data);
        this.data = data;
      },
      erro => {
        console.error('Erro ao carregar estoque:', erro);
        let mensagem = 'Erro ao carregar dados do estoque.';
        if (erro.status === 0) {
          mensagem += ' Verifique se a API está acessível.';
        } else if (erro.status === 404) {
          mensagem += ' Endpoint não encontrado.';
        }
        this.mostrarMensagem({
          title: 'Erro!',
          content: mensagem,
          cssClass: 'e-toast-danger'
        });
      }
    );
  }

  public onCodigoEstoqueBlur(event: any): void {
    const codigoEstoque = event.target.value;
    if (codigoEstoque && !this.isEdicao) {
      this.estoqueService.buscarPorCodigo(codigoEstoque).subscribe(
        estoque => {
          if (estoque) {
            this.mostrarMensagem({
              title: 'Aviso!',
              content: 'Código de estoque já existe.',
              cssClass: 'e-toast-warning'
            });
          }
        },
        erro => {
          if (erro.status !== 404) {
            console.error('Erro ao buscar estoque por código:', erro);
          }
        }
      );
    }
  }

  toolbarClick(args: any): void {
    if (args.item.id === 'grid_add') {
      this.isEdicao = false;
    }
  }

  actionBegin(args: any): void {
    console.log('Ação iniciada:', args);
    if (args.requestType === 'beginEdit') {
      this.isEdicao = true;
    }
    else if (args.requestType === 'save') {
      if (args.action === 'add') {
        const novoEstoque: EstoqueCreate = {
          codigoEstoque: args.data.codigoEstoque,
          quantidade: args.data.quantidade,
          localizacao: args.data.localizacao,
          produtoId: args.data.produtoId,
          usuarioModificacaoId: 1 // Usuário fixo até implementar autenticação
        };

        console.log('Dados do estoque para salvar:', novoEstoque);
        
        this.estoqueService.criar(novoEstoque).subscribe(
          (resultado) => {
            console.log('Estoque criado com sucesso:', resultado);
            this.mostrarMensagem({
              title: 'Sucesso!',
              content: 'Estoque cadastrado com sucesso.',
              cssClass: 'e-toast-success'
            });
            this.carregarDados();
          },
          erro => {
            console.error('Erro ao criar estoque:', erro);
            let mensagem = 'Erro ao cadastrar estoque.';
            if (erro.error?.message) {
              mensagem += ` ${erro.error.message}`;
            }
            this.mostrarMensagem({
              title: 'Erro!',
              content: mensagem,
              cssClass: 'e-toast-danger'
            });
          }
        );
      } else if (args.action === 'edit') {
        const estoqueAtualizado: Estoque = {
          id: args.data.id,
          codigoEstoque: args.data.codigoEstoque,
          quantidade: args.data.quantidade,
          localizacao: args.data.localizacao,
          dataAlteracao: new Date(),
          produtoId: args.data.produtoId,
          descricao: args.data.descricao,
          usuarioModificacaoId: 1, // Usuário fixo até implementar autenticação
          nome: args.data.nome
        };

        console.log('Dados do estoque para atualizar:', estoqueAtualizado);
        
        this.estoqueService.atualizar(estoqueAtualizado.id, estoqueAtualizado).subscribe(
          (resultado) => {
            console.log('Estoque atualizado com sucesso:', resultado);
            this.mostrarMensagem({
              title: 'Sucesso!',
              content: 'Estoque atualizado com sucesso.',
              cssClass: 'e-toast-success'
            });
            this.carregarDados();
          },
          erro => {
            console.error('Erro ao atualizar estoque:', erro);
            let mensagem = 'Erro ao atualizar estoque.';
            if (erro.error?.message) {
              mensagem += ` ${erro.error.message}`;
            }
            this.mostrarMensagem({
              title: 'Erro!',
              content: mensagem,
              cssClass: 'e-toast-danger'
            });
          }
        );
      }
    } else if (args.requestType === 'delete') {
      const estoque: Estoque = args.data[0];
      console.log('Dados do estoque para excluir:', estoque);
      this.estoqueService.excluir(estoque.id).subscribe(
        () => {
          console.log('Estoque excluído com sucesso');
          this.mostrarMensagem({
            title: 'Sucesso!',
            content: 'Estoque excluído com sucesso.',
            cssClass: 'e-toast-success'
          });
          this.carregarDados();
        },
        erro => {
          console.error('Erro ao excluir estoque:', erro);
          let mensagem = 'Erro ao excluir estoque.';
          if (erro.error?.message) {
            mensagem += ` ${erro.error.message}`;
          }
          this.mostrarMensagem({
            title: 'Erro!',
            content: mensagem,
            cssClass: 'e-toast-danger'
          });
        }
      );
    }
  }

  private mostrarMensagem(config: any): void {
    if (this.toastObj) {
      this.toastObj.show(config);
    }
  }

  public isFormValid(data: any): boolean {
    return data && 
           data.codigoEstoque && 
           data.quantidade > 0 && 
           data.localizacao;
  }

  public onSave(data: any): void {
    if (this.isFormValid(data)) {
      this.grid.endEdit();
    }
  }

  onEditar(data: Estoque): void {
    const estoqueAtualizado: Estoque = {
      id: data.id,
      codigoEstoque: data.codigoEstoque,
      quantidade: data.quantidade,
      localizacao: data.localizacao,
      dataAlteracao: new Date(),
      produtoId: data.produtoId,
      descricao: data.descricao,
      usuarioModificacaoId: 1,
      nome: data.nome
    };

    console.log('Dados do estoque para atualizar:', estoqueAtualizado);

    this.estoqueService.atualizar(estoqueAtualizado.id, estoqueAtualizado).subscribe(
      (resultado) => {
        console.log('Estoque atualizado com sucesso:', resultado);
        this.mostrarMensagem({
          title: 'Sucesso!',
          content: 'Estoque atualizado com sucesso.',
          cssClass: 'e-toast-success'
        });
        this.carregarDados();
      },
      erro => {
        console.error('Erro ao atualizar estoque:', erro);
        let mensagem = 'Erro ao atualizar estoque.';
        if (erro.error?.message) {
          mensagem += ` ${erro.error.message}`;
        }
        this.mostrarMensagem({
          title: 'Erro!',
          content: mensagem,
          cssClass: 'e-toast-danger'
        });
      }
    );
  }
} 