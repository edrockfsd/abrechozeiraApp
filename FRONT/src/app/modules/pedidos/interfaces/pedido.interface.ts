export interface Pedido {
  id?: number;
  numeroPedido: string;
  dataPedido: Date;
  statusPedidoId: number;
  pessoaId: number;
  enderecoId: number;
  observacao?: string;
  formaPagamento: string;
  parcelas: number;
  valorSubtotal: number;
  valorFrete: number;
  valorTotal: number;
  itens: ItemPedido[];
  usuarioModificacaoId: number;
}

export interface ItemPedido {
  id?: number;
  pedidoId?: number;
  sequencial: number;
  produtoId: number;
  produto: string;
  condicao: string;
  categoria: string;
  tamanho: string;
  cor: string;
  quantidade: number;
  valorUnitario: number;
  valorSubtotal: number;
}

export interface StatusPedido {
  id: number;
  descricao: string;
}

export interface FormaPagamento {
  id: number;
  descricao: string;
}

export interface Parcela {
  id: number;
  descricao: string;
  valor: number;
} 