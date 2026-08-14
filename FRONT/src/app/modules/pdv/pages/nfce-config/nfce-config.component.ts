import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NfceService, EmpresaFiscal, NfceValidacao } from '../../services/nfce.service';

@Component({
    selector: 'app-nfce-config',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterLink],
    templateUrl: './nfce-config.component.html',
    styleUrls: ['./nfce-config.component.scss']
})
export class NfceConfigComponent implements OnInit {
    config: EmpresaFiscal = {
        cnpj: '48.749.443/0001-06',
        inscricaoEstadual: '9123965768',
        razaoSocial: 'A BRECHOZEIRA LTDA',
        nomeFantasia: 'A Brechozeira',
        logradouro: 'Rua Julia Huga Maria Negrello',
        numero: '291',
        complemento: 'CSA 32 Cond San Francisco CD',
        bairro: 'Umbará',
        municipio: 'Curitiba',
        codigoMunicipio: '4106902',
        uf: 'PR',
        cep: '81930576',
        telefone: '',
        ambiente: 2, // Homologação
        crt: 1, // Simples Nacional
        serie: 1,
        proximoNumero: 1,
        tipoEmissao: 1,
        csc: 'I8AYSEH8REKZAPLJ96ZMOKFIVFVZD8RJ6RWD',
        cscId: '000001'
    };

    validacao: NfceValidacao | null = null;
    salvando = false;
    enviandoCertificado = false;
    carregando = true;
    mensagem: { tipo: 'success' | 'error'; texto: string } | null = null;

    ufs = ['AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG', 'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'];

    constructor(private nfceService: NfceService) { }

    onCertificadoSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (!input.files || input.files.length === 0) return;

        const file = input.files[0];
        this.enviandoCertificado = true;
        this.mensagem = null;

        this.nfceService.uploadCertificado(file, this.config.certificadoSenha).subscribe({
            next: (res) => {
                this.enviandoCertificado = false;
                this.config.certificadoPath = res.caminho;
                if (res.validade) {
                    this.config.certificadoValidade = res.validade;
                }
                this.mensagem = { tipo: 'success', texto: res.mensagem || 'Certificado enviado com sucesso!' };
            },
            error: (err) => {
                this.enviandoCertificado = false;
                this.mensagem = { tipo: 'error', texto: err.error?.erro || 'Erro ao enviar arquivo do certificado' };
            }
        });
    }

    ngOnInit(): void {
        this.carregarConfig();
    }

    carregarConfig(): void {
        this.carregando = true;
        this.nfceService.getConfig().subscribe({
            next: (config) => {
                if (config) {
                    this.config = { ...this.config, ...config };
                }
                this.carregando = false;
                this.validarConfiguracao();
            },
            error: () => {
                this.carregando = false;
            }
        });
    }

    validarConfiguracao(): void {
        this.nfceService.validarConfig().subscribe({
            next: (validacao) => {
                this.validacao = validacao;
            }
        });
    }

    salvar(): void {
        this.salvando = true;
        this.mensagem = null;

        this.nfceService.saveConfig(this.config).subscribe({
            next: (config) => {
                this.config = config;
                this.salvando = false;
                this.mensagem = { tipo: 'success', texto: 'Configurações salvas com sucesso!' };
                this.validarConfiguracao();
            },
            error: (err) => {
                this.salvando = false;
                this.mensagem = { tipo: 'error', texto: err.error?.erro || 'Erro ao salvar configurações' };
            }
        });
    }

    limparMensagem(): void {
        this.mensagem = null;
    }
}
