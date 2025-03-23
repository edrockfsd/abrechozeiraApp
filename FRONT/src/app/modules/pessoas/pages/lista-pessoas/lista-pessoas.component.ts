import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { GridComponent, GridModule, PagerModule } from '@syncfusion/ej2-angular-grids';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { PessoaService, PessoaGrid } from '../../services/pessoa.service';
import { ToastService } from '../../../../services/toast.service';

@Component({
  selector: 'app-lista-pessoas',
  templateUrl: './lista-pessoas.component.html',
  styleUrls: ['./lista-pessoas.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    GridModule,
    PagerModule,
    ButtonModule,
    ToastModule
  ]
})
export class ListaPessoasComponent implements OnInit, AfterViewInit {
  @ViewChild('grid') public grid: GridComponent;
  @ViewChild('toast') private toastObj: ToastComponent;

  public pessoas: PessoaGrid[] = [];
  public pageSettings = { pageSize: 10 };
  public toolbar = ['Search'];
  public filterSettings = { type: 'Excel' };

  public toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  constructor(
    private pessoaService: PessoaService,
    private router: Router,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.carregarPessoas();
  }

  ngAfterViewInit(): void {
    if (this.toastObj) {
      this.toastService.setToastComponent(this.toastObj);
    }
  }

  private carregarPessoas(): void {
    this.pessoaService.listarGrid().subscribe(
      (pessoas) => {
        this.pessoas = pessoas;
      },
      (erro) => {
        console.error('Erro ao carregar pessoas:', erro);
        this.toastService.showError('Erro ao carregar pessoas. Por favor, tente novamente.');
      }
    );
  }

  onNovaPessoa(): void {
    this.router.navigate(['/pessoas/novo']);
  }

  onEditar(pessoa: PessoaGrid): void {
    this.router.navigate(['/pessoas/editar', pessoa.id]);
  }

  onExcluir(pessoa: PessoaGrid): void {
    if (confirm('Tem certeza que deseja excluir esta pessoa?')) {
      this.pessoaService.excluir(pessoa.id).subscribe(
        () => {
          this.toastService.showSuccess('Pessoa excluída com sucesso!');
          this.carregarPessoas();
        },
        (erro) => {
          console.error('Erro ao excluir pessoa:', erro);
          this.toastService.showError('Erro ao excluir pessoa. Por favor, tente novamente.');
        }
      );
    }
  }

  formatarData(data: string): string {
    if (!data) return '';
    const d = new Date(data);
    return d.toLocaleDateString('pt-BR');
  }
} 