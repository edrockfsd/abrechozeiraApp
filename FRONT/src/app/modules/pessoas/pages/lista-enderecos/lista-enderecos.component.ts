import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { GridModule, PageService, SortService, FilterService, GroupService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { EnderecoService, Endereco } from '../../services/endereco.service';
import { TipoEnderecoService, TipoEndereco } from '../../services/tipo-endereco.service';

@Component({
  selector: 'app-lista-enderecos',
  templateUrl: './lista-enderecos.component.html',
  styleUrls: ['./lista-enderecos.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    ButtonModule,
    ToastModule
  ],
  providers: [
    PageService,
    SortService,
    FilterService,
    GroupService
  ]
})
export class ListaEnderecosComponent implements OnInit {
  @ViewChild('toast') public toast!: ToastComponent;
  @ViewChild('grid') public grid: any;

  public data: any[] = [];
  public pessoaId!: number;
  public carregando = true;
  public tiposEndereco: TipoEndereco[] = [];

  public gridSettings = {
    allowPaging: true,
    allowSorting: true,
    allowFiltering: true,
    allowGrouping: true,
    pageSettings: { pageSize: 10 },
    toolbar: ['Delete', 'Search'],
    editSettings: { allowDeleting: true, allowEditing: false, allowAdding: false }
  };

  public gridColumns = [
    { field: 'tipoEndereco', headerText: 'Tipo', width: 120 },
    { field: 'cep', headerText: 'CEP', width: 100 },
    { field: 'logradouro', headerText: 'Logradouro', width: 200 },
    { field: 'unidade', headerText: 'Número', width: 100 },
    { field: 'complemento', headerText: 'Complemento', width: 150 },
    { field: 'bairro', headerText: 'Bairro', width: 150 },
    { field: 'localidade', headerText: 'Cidade', width: 150 },
    { field: 'estado', headerText: 'Estado', width: 100 }
  ];

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private enderecoService: EnderecoService,
    private tipoEnderecoService: TipoEnderecoService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.pessoaId = +params['pessoaId'];
      this.carregarTiposEndereco();
      this.carregarEnderecos();
    });
  }

  private carregarTiposEndereco(): void {
    this.tipoEnderecoService.listar().subscribe({
      next: (tipos) => {
        console.log('Tipos de endereço recebidos:', tipos);
        this.tiposEndereco = tipos;
      },
      error: (error) => {
        console.error('Erro ao carregar tipos de endereço:', error);
        this.toast.show({
          title: 'Erro',
          content: 'Erro ao carregar tipos de endereço. Tente novamente.',
          cssClass: 'e-toast-danger',
          icon: 'e-error toast-icons'
        });
      }
    });
  }

  private carregarEnderecos(): void {
    this.carregando = true;
    this.enderecoService.listarPorPessoa(this.pessoaId).subscribe({
      next: (enderecos) => {
        console.log('Endereços recebidos:', enderecos);
        this.data = enderecos;
        this.carregando = false;
      },
      error: (error) => {
        console.error('Erro ao carregar endereços:', error);
        this.toast.show({
          title: 'Erro',
          content: 'Erro ao carregar endereços. Tente novamente.',
          cssClass: 'e-toast-danger',
          icon: 'e-error toast-icons'
        });
        this.carregando = false;
      }
    });
  }

  onActionComplete(args: any): void {
    if (args.requestType === 'save') {
      console.log('Dados do endereço:', args.data);
      
      // Encontrar o ID do tipo de endereço com base na descrição
      const tipoEndereco = this.tiposEndereco.find(
        tipo => tipo.descricao === args.data.tipoEndereco
      );
      
      if (!tipoEndereco) {
        this.toast.show({
          title: 'Erro',
          content: 'Tipo de endereço inválido. Tente novamente.',
          cssClass: 'e-toast-danger',
          icon: 'e-error toast-icons'
        });
        return;
      }
      
      const endereco: Endereco = {
        ...args.data,
        pessoaId: this.pessoaId,
        tipoEnderecoId: tipoEndereco.id,
        usuarioModificacaoId: 1 // Temporário até o módulo de usuários estar pronto
      };

      if (args.action === 'add') {
        this.enderecoService.criar(endereco).subscribe({
          next: () => {
            this.toast.show({
              title: 'Sucesso',
              content: 'Endereço cadastrado com sucesso!',
              cssClass: 'e-toast-success',
              icon: 'e-success toast-icons'
            });
            this.carregarEnderecos();
          },
          error: (error) => {
            console.error('Erro ao cadastrar endereço:', error);
            this.toast.show({
              title: 'Erro',
              content: 'Erro ao cadastrar endereço. Tente novamente.',
              cssClass: 'e-toast-danger',
              icon: 'e-error toast-icons'
            });
          }
        });
      } else if (args.action === 'edit') {
        this.enderecoService.atualizar(endereco.id!, endereco).subscribe({
          next: () => {
            this.toast.show({
              title: 'Sucesso',
              content: 'Endereço atualizado com sucesso!',
              cssClass: 'e-toast-success',
              icon: 'e-success toast-icons'
            });
            this.carregarEnderecos();
          },
          error: (error) => {
            console.error('Erro ao atualizar endereço:', error);
            this.toast.show({
              title: 'Erro',
              content: 'Erro ao atualizar endereço. Tente novamente.',
              cssClass: 'e-toast-danger',
              icon: 'e-error toast-icons'
            });
          }
        });
      }
    } else if (args.requestType === 'delete') {
      this.enderecoService.excluir(args.data[0].id).subscribe({
        next: () => {
          this.toast.show({
            title: 'Sucesso',
            content: 'Endereço excluído com sucesso!',
            cssClass: 'e-toast-success',
            icon: 'e-success toast-icons'
          });
          this.carregarEnderecos();
        },
        error: (error) => {
          console.error('Erro ao excluir endereço:', error);
          this.toast.show({
            title: 'Erro',
            content: 'Erro ao excluir endereço. Tente novamente.',
            cssClass: 'e-toast-danger',
            icon: 'e-error toast-icons'
          });
        }
      });
    }
  }

  onVoltar(): void {
    this.router.navigate(['/pessoas']);
  }

  onNovoEndereco(): void {
    this.router.navigate(['/pessoas', this.pessoaId, 'endereco', 'novo']);
  }

  onEditarEndereco(args: any): void {
    const enderecoId = args.data.id;
    this.router.navigate(['/pessoas', this.pessoaId, 'endereco', 'editar', enderecoId]);
  }
} 