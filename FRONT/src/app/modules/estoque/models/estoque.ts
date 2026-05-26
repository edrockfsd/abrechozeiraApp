export interface Estoque {
    id: number;
    codigoEstoque: string;
    quantidade: number;
    localizacao: string;
    dataAlteracao: Date;
    produtoId: number;
    descricao: string;
    usuarioModificacaoId: number;
    nome: string;
}

export interface EstoqueCreate {
    codigoEstoque: string;
    quantidade: number;
    localizacao: string;
    produtoId: number;
    usuarioModificacaoId: number;
}

export interface EstoqueDTO extends Estoque {
    produto?: {
        descricao: string;
    };
    usuarioModificacao?: {
        nome: string;
    };
} 