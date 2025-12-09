import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { CaixaService, CaixaStatus } from '../../services/caixa.service';
import { NumericTextBoxModule, TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { GridModule } from '@syncfusion/ej2-angular-grids';

@Component({
  selector: 'app-caixa-painel',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NumericTextBoxModule, TextBoxModule, ButtonModule, GridModule],
  templateUrl: './caixa-painel.component.html',
  styleUrls: ['./caixa-painel.component.scss']
})
export class CaixaPainelComponent implements OnInit, OnDestroy {
  constructor(private fb: FormBuilder, public caixa: CaixaService) {
    this.abrirForm = this.fb.group({
      saldoInicial: [0, [Validators.required, Validators.min(0)]],
      observacao: ['']
    });
    this.suprimentoForm = this.fb.group({
      valor: [0, [Validators.required, Validators.min(0.01)]],
      observacao: ['']
    });
    this.sangriaForm = this.fb.group({
      valor: [0, [Validators.required, Validators.min(0.01)]],
      observacao: ['']
    });
    this.fecharForm = this.fb.group({
      saldoFechamento: [0, [Validators.required, Validators.min(0)]]
    });
  }

  abrirForm: FormGroup;
  suprimentoForm: FormGroup;
  sangriaForm: FormGroup;
  fecharForm: FormGroup;

  current: CaixaStatus | null = null;
  private sub?: Subscription;

  ngOnInit(): void {
    this.sub = this.caixa.current$.subscribe(cx => {
      this.current = cx;
      if (cx) {
        this.caixa.loadMovimentos(cx.id).subscribe();
        this.fecharForm.patchValue({ saldoFechamento: cx.saldoInicial });
      } else {
        this.fecharForm.reset();
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  abrirCaixa() {
    if (this.abrirForm.invalid) return;
    this.caixa.abrirCaixa(this.abrirForm.value).subscribe(() => {
      this.abrirForm.reset();
    });
  }

  registrar(tipo: 'suprimento' | 'sangria') {
    const form = tipo === 'suprimento' ? this.suprimentoForm : this.sangriaForm;
    if (form.invalid) return;
    const { valor, observacao } = form.value;
    const action = tipo === 'suprimento'
      ? this.caixa.registrarSuprimento(valor, observacao)
      : this.caixa.registrarSangria(valor, observacao);
    action.subscribe(() => form.reset());
  }

  fecharCaixa() {
    if (this.fecharForm.invalid) return;
    this.caixa.fecharCaixa(this.fecharForm.value.saldoFechamento).subscribe();
  }
}
