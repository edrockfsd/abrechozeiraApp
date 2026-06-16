// ─── Cotação de Frete ───────────────────────────────────────────────────────────

export interface CotarFreteInput {
  cepOrigem: string;
  cepDestino: string;
  servicos: string;
  peso: number;
  altura: number;
  largura: number;
  comprimento: number;
  valorSeguro?: number;
}

export interface CotacaoFreteResultado {
  id: number;
  name: string;
  price: number;
  discount: string;
  currency: string;
  delivery_time: number;
  delivery_range: { min: number; max: number };
  packages?: PacoteResultado[];
  company?: EmpresaResultado;
  has_error: boolean;
}

export interface PacoteResultado {
  price: number;
  format: string;
  dimensions: { height: string; width: string; length: string };
  weight: string;
}

export interface EmpresaResultado {
  id: number;
  name: string;
  picture: string;
}

// ─── Criação de Etiqueta ─────────────────────────────────────────────────────────

export interface CriarEtiquetaInput {
  servicoId: number;
  // Destinatário
  destinatarioNome: string;
  destinatarioEndereco: string;
  destinatarioNumero: string;
  destinatarioBairro: string;
  destinatarioCidade: string;
  destinatarioEstado: string;
  destinatarioCep: string;
  destinatarioEmail?: string;
  destinatarioCpf?: string;
  // Dimensões
  peso: number;
  altura: number;
  largura: number;
  comprimento: number;
  // Produtos
  produtos: ProdutoEtiquetaInput[];
}

export interface ProdutoEtiquetaInput {
  nome: string;
  quantidade: number;
  valorUnitario: number;
}

export interface CriarEtiquetaResultado {
  id: string;
  price: number;
  status: string;
}

// ─── Etiqueta Gerada ─────────────────────────────────────────────────────────────

export interface EtiquetaInfo {
  id: string;
  protocol?: string;
  service_id: number;
  service_name: string;
  tracking?: string;
  price: number;
  status: string;
  created_at?: string;
  destinatario?: string;
  created_at_raw?: string;
  // Campos extras que adicionamos no front para exibição
  statusLabel?: string;
  statusClass?: string;
  statusPagamento?: string;
  statusSuperfrete?: string;
  email?: string;
  transacaoId?: string;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────────

export const SUPERFRETE_STATUS_MAP: Record<string, { label: string; cssClass: string }> = {
  pending:   { label: 'Aguardando Pagamento', cssClass: 'status-pending' },
  released:  { label: 'Aguardando Postagem',  cssClass: 'status-released' },
  posted:    { label: 'Postado',              cssClass: 'status-posted' },
  delivered: { label: 'Entregue',             cssClass: 'status-delivered' },
  cancelled: { label: 'Cancelado',            cssClass: 'status-cancelled' },
};

export const SUPERFRETE_SERVICOS: Record<number, string> = {
  1:  'PAC',
  2:  'SEDEX',
  17: 'Mini Envios',
  3:  'Jadlog',
  31: 'Loggi',
};
