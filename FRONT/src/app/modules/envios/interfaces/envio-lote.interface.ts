// ─── Parsing de Texto ─────────────────────────────────────────────────────────

export interface ParseTextoInput {
  texto: string;
}

export interface EnvioParseado {
  indice: number;
  nome: string;
  valor: number | null;
  pesoGramas: number;
  altura: number;
  largura: number;
  comprimento: number;
  clienteEncontrado: boolean;
  clienteId: number | null;
  clienteNomeSistema: string | null;
  enderecoId: number | null;
  cepDestino: string | null;
  enderecoCompleto: string | null;
  valido: boolean;
  erros: string[];
  // ─── Campos de controle no frontend ────────────────────────────────────────
  selecionado?: boolean;
  cepDestinoEdit?: string;
  destinatarioEndereco?: string;
  destinatarioNumero?: string;
  destinatarioBairro?: string;
  destinatarioCidade?: string;
  destinatarioEstado?: string;
  destinatarioEmail?: string;
  destinatarioCpf?: string;
  // Estado de cotação
  cotacaoFeita?: boolean;
  cotando?: boolean;
  cotacaoPrecoPAC?: number;
  cotacaoPrecoSEDEX?: number | null;
  cotacaoServicoId?: number;
  cotacaoServicoNome?: string;
  cotacaoPrecoEscolhido?: number;
  cotacaoMotivoEscolha?: string;
  cotacaoErro?: string;
  // Estado de etiqueta
  etiquetaGerada?: boolean;
  gerandoEtiqueta?: boolean;
  etiquetaId?: string;
  etiquetaStatus?: string;
  etiquetaPreco?: number;
  etiquetaErro?: string;
}

export interface ParseTextoResultado {
  envios: EnvioParseado[];
  naoProcessados: EnvioParseado[];
  totalParseados: number;
  totalValidos: number;
  totalInvalidos: number;
}

// ─── Cotação Separada ─────────────────────────────────────────────────────────

export interface EnvioParaCotar {
  indice: number;
  nome: string;
  valor: number;
  pesoGramas: number;
  altura: number;
  largura: number;
  comprimento: number;
  clienteId: number | null;
  enderecoId: number | null;
  cepDestino: string;
  destinatarioEndereco: string;
  destinatarioNumero: string;
  destinatarioBairro: string;
  destinatarioCidade: string;
  destinatarioEstado: string;
  destinatarioEmail?: string;
  destinatarioCpf?: string;
}

export interface CotarLoteInput {
  envios: EnvioParaCotar[];
}

export interface CotacaoLoteItem {
  indice: number;
  nome: string;
  precoPAC: number;
  precoSEDEX: number | null;
  servicoIdRecomendado: number;
  servicoRecomendado: string;
  precoRecomendado: number;
  motivoEscolha: string;
  sucesso: boolean;
  erro: string | null;
}

export interface CotarLoteResultado {
  resultados: CotacaoLoteItem[];
  totalSucesso: number;
  totalErro: number;
}

// ─── Geração de Etiquetas Separada ────────────────────────────────────────────

export interface EnvioParaGerarEtiqueta extends EnvioParaCotar {
  servicoId: number;
}

export interface GerarEtiquetasLoteInput {
  envios: EnvioParaGerarEtiqueta[];
}

export interface EtiquetaLoteItem {
  indice: number;
  nome: string;
  sucesso: boolean;
  etiquetaId: string | null;
  etiquetaStatus: string | null;
  etiquetaPreco: number | null;
  erro: string | null;
}

export interface GerarEtiquetasLoteResultado {
  resultados: EtiquetaLoteItem[];
  totalSucesso: number;
  totalErro: number;
  custoTotal: number;
}

// ─── Legacy (cotar + gerar junto) ─────────────────────────────────────────────

export interface CotarEGerarInput {
  envios: EnvioParaCotar[];
}

export interface EnvioLoteResultado {
  indice: number;
  nome: string;
  valor: number;
  servicoEscolhido: string;
  servicoId: number;
  precoPAC: number;
  precoSEDEX: number | null;
  precoEscolhido: number;
  motivoEscolha: string;
  etiquetaGerada: boolean;
  etiquetaId: string | null;
  etiquetaStatus: string | null;
  etiquetaPreco: number | null;
  sucesso: boolean;
  erro: string | null;
}

export interface CotarEGerarResultado {
  resultados: EnvioLoteResultado[];
  totalSucesso: number;
  totalErro: number;
  custoTotal: number;
}
