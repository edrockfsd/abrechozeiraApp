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
        cnpj: '',
        inscricaoEstadual: '',
        razaoSocial: '',
        nomeFantasia: '',
        logradouro: '',
        numero: '',
        complemento: '',
        bairro: '',
        municipio: '',
        codigoMunicipio: '',
        uf: 'RS',
        cep: '',
        telefone: '',
        ambiente: 2, // Homologação
        crt: 4, // MEI
        serie: 1,
        tipoEmissao: 1,
        csc: '',
        cscId: ''
    };

    validacao: NfceValidacao | null = null;
    salvando = false;
    carregando = true;
    mensagem: { tipo: 'success' | 'error'; texto: string } | null = null;

    ufs = ['AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG', 'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'];

    constructor(private nfceService: NfceService) { }

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
