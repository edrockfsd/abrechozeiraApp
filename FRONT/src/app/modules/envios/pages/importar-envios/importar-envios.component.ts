import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ToastComponent, ToastModule } from '@syncfusion/ej2-angular-notifications';
import { EnvioLoteService } from '../../services/envio-lote.service';
import {
  EnvioParseado,
  EnvioParaCotar,
  EnvioParaGerarEtiqueta
} from '../../interfaces/envio-lote.interface';

type Etapa = 'input' | 'review';

@Component({
  selector: 'app-importar-envios',
  templateUrl: './importar-envios.component.html',
  styleUrls: ['./importar-envios.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    ToastModule
  ]
})
export class ImportarEnviosComponent {
  @ViewChild('toast') toast!: ToastComponent;

  etapaAtual: Etapa = 'input';
  textoWhatsApp = '';
  processando = false;

  // Resultados do parsing
  enviosValidos: EnvioParseado[] = [];
  enviosInvalidos: EnvioParseado[] = [];
  totalParseados = 0;

  // Seleção
  selecionarTodos = false;

  // Bulk operation loading
  cotandoLote = false;
  gerandoLote = false;
  enviandoRastreiosLote = false;

  toastSettings = {
    position: { X: 'Right', Y: 'Top' },
    showCloseButton: true,
    timeOut: 5000
  };

  constructor(
    private envioLoteService: EnvioLoteService,
    private router: Router
  ) {}

  // ─── Atualização de Status em Lote ─────────────────────────────────────────

  verificandoStatus = false;

  onVerificarStatusSelecionados(): void {
    if (this.selecionados.length === 0) return;

    // Pega apenas os que têm transacaoId gerado (já cotados)
    const transacaoIds = this.selecionados
      .filter(e => (e as any).transacaoId)
      .map(e => (e as any).transacaoId!);

    if (transacaoIds.length === 0) {
      this.showToast('Atenção', 'Nenhum dos selecionados foi cotado ainda.', 'e-toast-warning');
      return;
    }

    this.verificandoStatus = true;
    this.envioLoteService.verificarStatus(transacaoIds).subscribe({
      next: (resultados) => {
        resultados.forEach(res => {
          const envio = this.enviosValidos.find(e => (e as any).transacaoId === res.transacaoId);
          if (envio) {
            (envio as any).statusPagamento = res.statusPagamento;
            (envio as any).statusSuperfrete = res.statusSuperfrete;
            if (res.statusSuperfrete === 'Liberada') {
              envio.etiquetaStatus = 'Liberada';
            }
          }
        });
        this.verificandoStatus = false;
        this.showToast('Sucesso', 'Status atualizados!', 'e-toast-success');
      },
      error: (err) => {
        console.error('Erro ao verificar status:', err);
        this.verificandoStatus = false;
        this.showToast('Erro', 'Falha ao sincronizar status com o servidor.', 'e-toast-danger');
      }
    });
  }

  // ─── Envio de Rastreios em Lote ────────────────────────────────────────────

  onEnviarRastreiosSelecionados(): void {
    const enviosParaRastreio = this.selecionados.filter(e => e.etiquetaGerada && e.etiquetaId && e.destinatarioEmail);

    if (enviosParaRastreio.length === 0) {
      this.showToast('Atenção', 'Nenhum envio selecionado possui etiqueta gerada e e-mail cadastrado.', 'e-toast-warning');
      return;
    }

    this.enviandoRastreiosLote = true;

    const payload = {
      envios: enviosParaRastreio.map(e => ({
        etiquetaId: e.etiquetaId!,
        email: e.destinatarioEmail!,
        nome: e.nome
      }))
    };

    this.envioLoteService.enviarRastreioLote(payload).subscribe({
      next: (res) => {
        this.enviandoRastreiosLote = false;
        
        if (res.totalErro > 0) {
          const nomesErros = res.erros.map((err: any) => `${err.nome} (${err.erro})`).join(', ');
          this.showToast(
            'Atenção',
            `${res.totalSucesso} e-mail(s) enviado(s). Houve erro(s) em ${res.totalErro} envio(s): ${nomesErros}`,
            'e-toast-warning'
          );
        } else {
          this.showToast('Sucesso', `${res.totalSucesso} e-mail(s) de rastreio enviado(s)!`, 'e-toast-success');
        }
      },
      error: (err) => {
        console.error('Erro ao enviar rastreios em lote:', err);
        this.enviandoRastreiosLote = false;
        this.showToast('Erro', 'Falha ao processar envio de rastreios.', 'e-toast-danger');
      }
    });
  }

  // ─── Etapa 1: Processar Texto ──────────────────────────────────────────────

  onProcessarTexto(): void {
    if (!this.textoWhatsApp.trim()) {
      this.showToast('Atenção', 'Cole o texto do WhatsApp antes de processar.', 'e-toast-warning');
      return;
    }

    this.processando = true;

    this.envioLoteService.parseTexto({ texto: this.textoWhatsApp }).subscribe({
      next: (resultado) => {
        this.enviosValidos = resultado.envios.map(e => ({ ...e, selecionado: false }));
        this.enviosInvalidos = resultado.naoProcessados;
        this.totalParseados = resultado.totalParseados;
        this.processando = false;

        if (resultado.totalValidos > 0) {
          this.etapaAtual = 'review';
          this.showToast(
            'Sucesso',
            `${resultado.totalValidos} envio(s) identificado(s)${resultado.totalInvalidos > 0 ? `, ${resultado.totalInvalidos} com problemas` : ''}.`,
            'e-toast-success'
          );
        } else {
          this.showToast('Atenção', 'Nenhum envio válido encontrado.', 'e-toast-warning');
        }
      },
      error: (err) => {
        console.error('Erro ao processar texto:', err);
        this.processando = false;
        this.showToast('Erro', 'Erro ao processar o texto.', 'e-toast-danger');
      }
    });
  }

  onLimparTexto(): void {
    this.textoWhatsApp = '';
  }

  // ─── Seleção ───────────────────────────────────────────────────────────────

  onSelecionarTodos(): void {
    this.enviosValidos.forEach(e => e.selecionado = this.selecionarTodos);
  }

  onSelecaoItemChange(): void {
    this.selecionarTodos = this.enviosValidos.length > 0 && this.enviosValidos.every(e => e.selecionado);
  }

  get selecionados(): EnvioParseado[] {
    return this.enviosValidos.filter(e => e.selecionado);
  }

  get temSelecionados(): boolean {
    return this.selecionados.length > 0;
  }

  // ─── Excluir ───────────────────────────────────────────────────────────────

  onExcluirItem(envio: EnvioParseado): void {
    this.enviosValidos = this.enviosValidos.filter(e => e.indice !== envio.indice);
    this.onSelecaoItemChange();
    this.showToast('Removido', `${envio.nome} removido da lista.`, 'e-toast-info');
  }

  onExcluirSelecionados(): void {
    const count = this.selecionados.length;
    const indices = new Set(this.selecionados.map(e => e.indice));
    this.enviosValidos = this.enviosValidos.filter(e => !indices.has(e.indice));
    this.selecionarTodos = false;
    this.showToast('Removidos', `${count} envio(s) removido(s).`, 'e-toast-info');
  }

  // ─── Cotar Individual ──────────────────────────────────────────────────────

  onCotarItem(envio: EnvioParseado): void {
    if (this.envioSemCep(envio)) {
      this.showToast('Atenção', 'Preencha o CEP de destino antes de cotar.', 'e-toast-warning');
      return;
    }

    envio.cotando = true;
    envio.cotacaoErro = undefined;

    const payload = this.buildCotarPayload(envio);

    this.envioLoteService.cotarLote({ envios: [payload] }).subscribe({
      next: (res) => {
        const item = res.resultados[0];
        if (item?.sucesso) {
          envio.cotacaoFeita = true;
          envio.cotacaoPrecoPAC = item.precoPAC;
          envio.cotacaoPrecoSEDEX = item.precoSEDEX;
          envio.cotacaoServicoId = item.servicoIdRecomendado;
          envio.cotacaoServicoNome = item.servicoRecomendado;
          envio.cotacaoPrecoEscolhido = item.precoRecomendado;
          envio.cotacaoServicoNome = item.servicoRecomendado;
          envio.cotacaoPrecoEscolhido = item.precoRecomendado;
          envio.cotacaoMotivoEscolha = item.motivoEscolha;
          (envio as any).transacaoId = item.transacaoId; // Armazena transacaoId

          // Notificar que e-mail foi enviado (se cliente tem email)
          if (envio.destinatarioEmail) {
            this.showToast('📧 E-mail', `Cotação enviada por e-mail para ${envio.nome}`, 'e-toast-info');
          }
        } else {
          envio.cotacaoErro = item?.erro || 'Erro desconhecido';
          this.showToast('Erro', `Cotação falhou para ${envio.nome}: ${envio.cotacaoErro}`, 'e-toast-danger');
        }
        envio.cotando = false;
      },
      error: (err) => {
        envio.cotando = false;
        envio.cotacaoErro = 'Erro de comunicação';
        this.showToast('Erro', `Erro ao cotar ${envio.nome}.`, 'e-toast-danger');
      }
    });
  }

  onCotarSelecionados(): void {
    const enviosParaCotar = this.selecionados.filter(e => !this.envioSemCep(e));
    if (enviosParaCotar.length === 0) {
      this.showToast('Atenção', 'Nenhum envio selecionado com CEP preenchido.', 'e-toast-warning');
      return;
    }

    this.cotandoLote = true;
    enviosParaCotar.forEach(e => { e.cotando = true; e.cotacaoErro = undefined; });

    const payloads = enviosParaCotar.map(e => this.buildCotarPayload(e));

    this.envioLoteService.cotarLote({ envios: payloads }).subscribe({
      next: (res) => {
        res.resultados.forEach(item => {
          const envio = this.enviosValidos.find(e => e.indice === item.indice);
          if (!envio) return;
          if (item.sucesso) {
            envio.cotacaoFeita = true;
            envio.cotacaoPrecoPAC = item.precoPAC;
            envio.cotacaoPrecoSEDEX = item.precoSEDEX;
            envio.cotacaoServicoId = item.servicoIdRecomendado;
            envio.cotacaoServicoNome = item.servicoRecomendado;
            envio.cotacaoServicoNome = item.servicoRecomendado;
            envio.cotacaoPrecoEscolhido = item.precoRecomendado;
            envio.cotacaoMotivoEscolha = item.motivoEscolha;
            (envio as any).transacaoId = item.transacaoId; // Armazena transacaoId
          } else {
            envio.cotacaoErro = item.erro || 'Erro';
          }
          envio.cotando = false;
        });
        this.cotandoLote = false;
        this.showToast('Cotação', `${res.totalSucesso} cotação(ões) realizada(s).`, 'e-toast-success');
      },
      error: () => {
        enviosParaCotar.forEach(e => { e.cotando = false; e.cotacaoErro = 'Erro de comunicação'; });
        this.cotandoLote = false;
        this.showToast('Erro', 'Erro ao cotar em lote.', 'e-toast-danger');
      }
    });
  }

  // ─── Gerar Etiqueta Individual ─────────────────────────────────────────────

  onGerarEtiquetaItem(envio: EnvioParseado): void {
    if (!envio.cotacaoFeita || !envio.cotacaoServicoId) {
      this.showToast('Atenção', 'Cote o frete antes de gerar a etiqueta.', 'e-toast-warning');
      return;
    }
    if (envio.etiquetaGerada) {
      this.showToast('Info', 'Etiqueta já gerada para este envio.', 'e-toast-info');
      return;
    }

    envio.gerandoEtiqueta = true;
    envio.etiquetaErro = undefined;

    const payload = this.buildEtiquetaPayload(envio);

    this.envioLoteService.gerarEtiquetasLote({ envios: [payload] }).subscribe({
      next: (res) => {
        const item = res.resultados[0];
        if (item?.sucesso) {
          envio.etiquetaGerada = true;
          envio.etiquetaId = item.etiquetaId || undefined;
          envio.etiquetaStatus = item.etiquetaStatus || undefined;
          envio.etiquetaPreco = item.etiquetaPreco || undefined;
          this.showToast('Sucesso', `Etiqueta gerada para ${envio.nome}!`, 'e-toast-success');

          // Notificar sobre e-mail de rastreio
          if (envio.destinatarioEmail) {
            this.showToast('📧 Rastreio', `Código de rastreio será enviado por e-mail para ${envio.nome}`, 'e-toast-info');
          }
        } else {
          envio.etiquetaErro = item?.erro || 'Erro desconhecido';
          this.showToast('Erro', `Erro ao gerar etiqueta: ${envio.etiquetaErro}`, 'e-toast-danger');
        }
        envio.gerandoEtiqueta = false;
      },
      error: () => {
        envio.gerandoEtiqueta = false;
        envio.etiquetaErro = 'Erro de comunicação';
        this.showToast('Erro', `Erro ao gerar etiqueta para ${envio.nome}.`, 'e-toast-danger');
      }
    });
  }

  onGerarEtiquetasSelecionados(): void {
    const enviosParaGerar = this.selecionados.filter(e => e.cotacaoFeita && e.cotacaoServicoId && !e.etiquetaGerada);
    if (enviosParaGerar.length === 0) {
      this.showToast('Atenção', 'Nenhum envio selecionado com cotação feita (e sem etiqueta).', 'e-toast-warning');
      return;
    }

    this.gerandoLote = true;
    enviosParaGerar.forEach(e => { e.gerandoEtiqueta = true; e.etiquetaErro = undefined; });

    const payloads = enviosParaGerar.map(e => this.buildEtiquetaPayload(e));

    this.envioLoteService.gerarEtiquetasLote({ envios: payloads }).subscribe({
      next: (res) => {
        res.resultados.forEach(item => {
          const envio = this.enviosValidos.find(e => e.indice === item.indice);
          if (!envio) return;
          if (item.sucesso) {
            envio.etiquetaGerada = true;
            envio.etiquetaId = item.etiquetaId || undefined;
            envio.etiquetaStatus = item.etiquetaStatus || undefined;
            envio.etiquetaPreco = item.etiquetaPreco || undefined;
          } else {
            envio.etiquetaErro = item.erro || 'Erro';
          }
          envio.gerandoEtiqueta = false;
        });
        this.gerandoLote = false;
        this.showToast('Etiquetas', `${res.totalSucesso} etiqueta(s) gerada(s). Custo: R$ ${res.custoTotal.toFixed(2)}`, 'e-toast-success');
      },
      error: () => {
        enviosParaGerar.forEach(e => { e.gerandoEtiqueta = false; e.etiquetaErro = 'Erro'; });
        this.gerandoLote = false;
        this.showToast('Erro', 'Erro ao gerar etiquetas em lote.', 'e-toast-danger');
      }
    });
  }

  // ─── Builders de payload ───────────────────────────────────────────────────

  private buildCotarPayload(e: EnvioParseado): EnvioParaCotar {
    return {
      indice: e.indice,
      nome: e.nome,
      valor: e.valor!,
      pesoGramas: e.pesoGramas,
      altura: e.altura,
      largura: e.largura,
      comprimento: e.comprimento,
      clienteId: e.clienteId,
      enderecoId: e.enderecoId,
      cepDestino: (e.cepDestinoEdit || e.cepDestino || '').replace(/\D/g, ''),
      destinatarioEndereco: e.destinatarioEndereco || '',
      destinatarioNumero: e.destinatarioNumero || '',
      destinatarioBairro: e.destinatarioBairro || '',
      destinatarioCidade: e.destinatarioCidade || '',
      destinatarioEstado: e.destinatarioEstado || '',
      destinatarioEmail: e.destinatarioEmail,
      destinatarioCpf: e.destinatarioCpf
    };
  }

  private buildEtiquetaPayload(e: EnvioParseado): EnvioParaGerarEtiqueta {
    return {
      ...this.buildCotarPayload(e),
      servicoId: e.cotacaoServicoId!,
      transacaoId: (e as any).transacaoId
    };
  }

  // ─── Reenviar Rastreio por E-mail ─────────────────────────────────────────────────

  onReenviarRastreio(envio: EnvioParseado): void {
    if (!envio.etiquetaId || !envio.destinatarioEmail) {
      this.showToast('Atenção', 'Etiqueta ou e-mail não disponíveis para reenvio.', 'e-toast-warning');
      return;
    }

    this.envioLoteService.enviarRastreio(envio.etiquetaId, envio.destinatarioEmail, envio.nome).subscribe({
      next: (res) => {
        this.showToast('📧 Enviado', res.message, 'e-toast-success');
      },
      error: (err) => {
        const msg = err.error?.message || 'Erro ao reenviar rastreio.';
        this.showToast('Erro', msg, 'e-toast-danger');
      }
    });
  }

  // ─── Navigation ────────────────────────────────────────────────────────────

  onVoltarParaInput(): void {
    this.etapaAtual = 'input';
  }

  onNovaImportacao(): void {
    this.etapaAtual = 'input';
    this.textoWhatsApp = '';
    this.enviosValidos = [];
    this.enviosInvalidos = [];
    this.totalParseados = 0;
    this.selecionarTodos = false;
  }

  onIrParaEnvios(): void {
    this.router.navigate(['/envios']);
  }

  // ─── Helpers ───────────────────────────────────────────────────────────────

  envioSemCep(envio: EnvioParseado): boolean {
    const cep = envio.cepDestinoEdit || envio.cepDestino || '';
    return cep.replace(/\D/g, '').length < 8;
  }

  formatarDimensoes(envio: EnvioParseado): string {
    return `${envio.altura}×${envio.largura}×${envio.comprimento}`;
  }

  formatarPeso(gramas: number): string {
    if (gramas >= 1000) return `${(gramas / 1000).toFixed(1)}kg`;
    return `${gramas}g`;
  }

  formatarValor(valor: number | null): string {
    if (valor === null || valor === undefined) return 'R$ ?';
    return `R$ ${valor.toFixed(2).replace('.', ',')}`;
  }

  get etiquetasGeradas(): number {
    return this.enviosValidos.filter(e => e.etiquetaGerada).length;
  }

  get custoTotalEtiquetas(): number {
    return this.enviosValidos
      .filter(e => e.etiquetaGerada && e.etiquetaPreco)
      .reduce((sum, e) => sum + (e.etiquetaPreco || 0), 0);
  }

  private showToast(title: string, content: string, cssClass: string): void {
    if (this.toast) {
      this.toast.show({ title, content, cssClass, icon: '' });
    }
  }
}
