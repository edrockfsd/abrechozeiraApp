import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ComboBoxModule } from '@syncfusion/ej2-angular-dropdowns';
import { GridComponent, GridModule } from '@syncfusion/ej2-angular-grids';
import { ArremateService } from '../../services/arremate.service';
import { Arremate, ArremateRequest } from '../../services/arremate.service';
// Add ToastModule import
import { ToastModule, ToastComponent } from '@syncfusion/ej2-angular-notifications';

import { ToolbarItems } from '@syncfusion/ej2-angular-grids';

interface LiveCombo {
  id: number;
  titulo: string;
}

@Component({
  selector: 'app-arremate',
  templateUrl: './arremate.component.html',
  styleUrls: ['./arremate.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ComboBoxModule,
    GridModule,
    ToastModule // Add ToastModule to imports
  ]
})
export class ArremateComponent implements OnInit {
  @ViewChild('grid') public grid: GridComponent;
  @ViewChild('toast') public toast: ToastComponent; // Add toast reference

  // Add toast configuration
  public toastPosition = { X: 'Right', Y: 'Top' };
  public toastWidth = '400';
  arremateForm: FormGroup;
  lives: LiveCombo[] = [];
  arremates: Arremate[] = [];
  public liveFields: object = { text: 'titulo', value: 'id' };
  public pageSettings = { pageSize: 10 };
  public toolbar = ['Add', 'Search'];
  public filterSettings = { type: 'Excel' };

  constructor(
    private formBuilder: FormBuilder,
    private arremateService: ArremateService
  ) {
    this.criarFormulario();
  }

  public toolbarOptions: string[];
  public editSettings: object;

  ngOnInit(): void {
    this.carregarLives();
    this.toolbarOptions = ['Delete'];
    this.editSettings = { 
      allowEditing: false, 
      allowAdding: false, 
      allowDeleting: true,
      mode: 'Dialog',
      primaryKey: 'id'
    };
  }

  private criarFormulario(): void {
    this.arremateForm = this.formBuilder.group({
      liveId: ['', [Validators.required]]
    });
  }

  private carregarLives(): void {
    this.arremateService.getLivesCombo().subscribe(
      (data) => {
        this.lives = data;
      },
      (erro) => {
        console.error('Erro ao carregar lives:', erro);
      }
    );
  }

  selectedLiveId: number | null = null;

  onLiveChange(event: any): void {
    this.selectedLiveId = event.value;
    if (event.value) {
      this.carregarArremates(event.value);
    } else {
      this.arremates = [];
    }
    // Reset quick entry fields when live changes
    const codigoEstoqueInput = document.getElementById('quickCodigoEstoque') as HTMLInputElement;
    const pecaInput = document.getElementById('quickPeca') as HTMLInputElement;
    const codigoLiveInput = document.getElementById('quickCodigoLive') as HTMLInputElement;
    const precoVendaInput = document.getElementById('quickPrecoVenda') as HTMLInputElement;
    const arrematanteInput = document.getElementById('quickArrematante') as HTMLInputElement;
    const filaInput = document.getElementById('quickFila') as HTMLTextAreaElement;
    [codigoEstoqueInput, pecaInput, codigoLiveInput, precoVendaInput, arrematanteInput].forEach(inp => {
      if (inp) {
        inp.value = '';
      }
    });
    if (filaInput) filaInput.value = '';
    if (pecaInput) pecaInput.readOnly = false;
    if (precoVendaInput) precoVendaInput.readOnly = false;
    this.selectedProdutoId = null;
  }

  private carregarArremates(liveId: number): void {
    this.arremateService.getArrematesByLiveId(liveId).subscribe(
      (data) => {
        this.arremates = data;
      },
      (erro) => {
        console.error('Erro ao carregar arremates:', erro);
      }
    );
  }

  onActionBegin(event: any): void {
    if (event.requestType === 'delete') {
      const id = event.data[0].id;
      this.arremateService.deleteArremate(id).subscribe({
        next: () => {
          this.showToast('success', 'Sucesso', 'Arremate excluído com sucesso');
          this.carregarArremates(this.selectedLiveId!);
        },
        error: (error) => {
          console.error('Erro ao excluir arremate:', error);
          this.showToast('error', 'Erro', 'Erro ao excluir arremate');
          event.cancel = true;
        }
      });
    }
    if (event.requestType === 'add') {
      const liveId = this.arremateForm.get('liveId')?.value;
      if (!liveId) {
        event.cancel = true;
        return;
      }

      const codigoEstoque = event.data.codigoEstoque;
      if (codigoEstoque) {
        this.arremateService.getProdutoByCodigoEstoque(codigoEstoque).subscribe(
          (produto) => {
            event.data.codigoLive = liveId;
            event.data.produtoDescricao = produto.descricao;
            event.data.valorArremate = produto.precoVenda;
            event.data.dataArremate = new Date().toISOString();
          },
          (erro) => {
            console.error('Erro ao buscar produto:', erro);
            event.cancel = true;
          }
        );
      }
    }
  }

  // Add this property to store the product ID
  selectedProdutoId: number | null = null;

  onCodigoEstoqueBlur(event: any): void {
    const codigoEstoque = (event.target.value || '').toString().trim();
    const pecaInput = document.getElementById('quickPeca') as HTMLInputElement;
    const precoVendaInput = document.getElementById('quickPrecoVenda') as HTMLInputElement;

    if (!codigoEstoque) {
      // Sem código -> habilita entrada manual
      this.selectedProdutoId = null;
      if (pecaInput) pecaInput.readOnly = false;
      if (precoVendaInput) precoVendaInput.readOnly = false;
      return;
    }

    this.arremateService.getProdutoByCodigoEstoque(codigoEstoque)
      .subscribe({
        next: (response: any) => {
          // Caso API retorne objeto com Id=0 (não encontrado), NÃO travar campos
          const found = response && typeof response.id === 'number' && response.id > 0;

          if (found) {
            this.selectedProdutoId = response.id;
            if (pecaInput && response.descricao) {
              pecaInput.value = response.descricao;
            }
            if (precoVendaInput && response.precoVenda != null) {
              precoVendaInput.value = Number(response.precoVenda).toLocaleString('pt-BR', {
                style: 'currency',
                currency: 'BRL'
              });
            }
          } else {
            // Não encontrado: liberar edição manual e manter/limpar valores
            this.selectedProdutoId = null;
            const codigoEstoqueInput = document.getElementById('quickCodigoEstoque') as HTMLInputElement;
            if (codigoEstoqueInput) {
              codigoEstoqueInput.value = '';
            }
            if (pecaInput) {
              // Evita deixar "Produto não encontrado" como texto
              if (!pecaInput.value || (response?.descricao && /n.o encontrado/i.test(response.descricao))) {
                pecaInput.value = '';
              }
              pecaInput.readOnly = false;
            }
            if (precoVendaInput) {
              if (response?.precoVenda === 0 && !precoVendaInput.value) {
                precoVendaInput.value = '';
              }
              precoVendaInput.readOnly = false;
            }
            this.showToast('warning', 'Produto não encontrado', 'Informe a descrição e o preço manualmente.');
          }
        },
        error: (error) => {
          // Reset the product ID on error e permitir entrada manual
          this.selectedProdutoId = null;
          console.error('Erro ao buscar produto:', error);
          const codigoEstoqueInput = document.getElementById('quickCodigoEstoque') as HTMLInputElement;
          if (codigoEstoqueInput) {
            codigoEstoqueInput.value = '';
          }
          if (pecaInput) pecaInput.readOnly = false;
          if (precoVendaInput) precoVendaInput.readOnly = false;
        }
      });
  }

  onAddButtonClick(): void {
    // Validate all required fields
    const missingFields = [];

    const codigoEstoqueInput = document.getElementById('quickCodigoEstoque') as HTMLInputElement;
    const pecaInput = document.getElementById('quickPeca') as HTMLInputElement;
    const codigoLiveInput = document.getElementById('quickCodigoLive') as HTMLInputElement;
    const precoVendaInput = document.getElementById('quickPrecoVenda') as HTMLInputElement;
    const arrematanteInput = document.getElementById('quickArrematante') as HTMLInputElement;
    const filaInput = document.getElementById('quickFila') as HTMLTextAreaElement;
    
    if (!this.selectedLiveId) {
      missingFields.push('Live');
    }

    if (!codigoLiveInput.value.trim()) {
      missingFields.push('Código Live');
    }
    
    if (!pecaInput.value.trim()) {
      missingFields.push('Peça');
    }
    
    if (!precoVendaInput.value.trim()) {
      missingFields.push('Preço Venda');
    }
    
    if (!arrematanteInput.value.trim()) {
      missingFields.push('Arrematante');
    }
    
    if (missingFields.length > 0) {
      this.showToast('error', 'Campos obrigatórios', `Os seguintes campos são obrigatórios: ${missingFields.join(', ')}`);
      return;
    }
    
    // ProdutoId passa a ser opcional (pode ser null)
    // Remove currency formatting and convert to number
    const codigoLiveStr = codigoLiveInput.value.trim();
    const codigoLive = parseInt(codigoLiveStr);
    const precoVendaStr = precoVendaInput.value.replace('R$', '').replace('.', '').replace(',', '.').trim();
    const valorArremate = parseFloat(precoVendaStr);

    const arremateRequest: ArremateRequest = {
      liveId: this.selectedLiveId,
      produtoId: this.selectedProdutoId ?? null,
      descricaoManual: this.selectedProdutoId ? null : pecaInput.value,
      codigoLive: codigoLive,
      arrematante: arrematanteInput.value,
      valorArremate: valorArremate,
      observacoes: filaInput.value,
      dataArremate: new Date().toISOString(),
      dataAlteracao: new Date().toISOString(),
      usuarioModificacaoId: 1
    };
    console.log(arremateRequest);

    this.arremateService.createArremate(arremateRequest).subscribe({
      next: (response) => {
        console.log('Arremate criado com sucesso:', response);
        // Show success toast
        this.showToast('success', 'Sucesso', 'Arremate criado com sucesso');
        
        // Clear all form fields
        codigoEstoqueInput.value = '';
        codigoLiveInput.value = '';
        pecaInput.value = '';
        precoVendaInput.value = '';
        arrematanteInput.value = '';
        filaInput.value = '';
        // Re-enable manual editing by default
        pecaInput.readOnly = false;
        precoVendaInput.readOnly = false;
        
        // Reset the product ID
        this.selectedProdutoId = null;
        
        // Refresh the grid
        this.carregarArremates(this.selectedLiveId!);
      },
      error: (error) => {
        console.error('Erro ao criar arremate:', error);
        this.showToast('error', 'Erro', 'Erro ao criar arremate');
      }
    });
  }

  // Add toast method
  showToast(severity: string, title: string, message: string): void {
    this.toast.show({
      title: title,
      content: message,
      cssClass: `e-toast-${severity}`,
      icon: severity === 'error' ? 'e-error toast-icons' : 'e-success toast-icons',
      position: this.toastPosition,
      timeOut: 5000,
      showCloseButton: true,
      animation: { show: { effect: 'FadeIn' }, hide: { effect: 'FadeOut' } }
    });
  }




  formatarData(data: string): string {
    if (!data) return '';
    const d = new Date(data);
    return d.toLocaleString('pt-BR');
  }

  // Formatação de moeda BRL no input conforme digita
  onPrecoVendaInput(event: any): void {
    const input = event.target as HTMLInputElement;
    if (input.readOnly) return;

    // Mantém apenas dígitos
    const cleaned = (input.value || '').replace(/\D/g, '');
    // Remove zeros à esquerda para evitar formatos como 000.299,00
    const digits = cleaned.replace(/^0+/, '') || '0';
    if (!digits) {
      input.value = '';
      return;
    }

    // Converte para centavos (últimos 2 dígitos)
    const cents = digits.padStart(3, '0');
    const inteiro = cents.slice(0, -2);
    const decimal = cents.slice(-2);

    // Aplica máscara BRL: R$ 0.000,00
    const inteiroFormatado = inteiro.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    input.value = `R$ ${inteiroFormatado},${decimal}`;
  }

  onPrecoVendaBlur(event: any): void {
    const input = event.target as HTMLInputElement;
    if (!input.value) return;
    // Garante sempre formato R$ 0,00
    const onlyDigits = input.value.replace(/\D/g, '').replace(/^0+/, '') || '0';
    const cents = onlyDigits.padStart(3, '0');
    const inteiro = cents.slice(0, -2);
    const decimal = cents.slice(-2);
    const inteiroFormatado = inteiro.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    input.value = `R$ ${inteiroFormatado},${decimal}`;
  }
}
