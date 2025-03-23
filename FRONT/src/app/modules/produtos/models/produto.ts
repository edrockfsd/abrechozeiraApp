export interface Produto {
    id?: number;
    descricao: string;
    tamanho: string;
    precoCusto: number;
    precoVenda: number;
    origem: string;
    grupoID: number;
    dataCompra: Date;
    dataAlteracao?: Date;
    usuarioModificacaoId?: number;
    statusId: number;
    marcaId: number;
    generoID: number;
    perfilID: number;
    produtoGrupoID: number;
}

// Enums para os status possíveis
export enum ProdutoStatus {
    Ativo = 1,
    Inativo = 2,
    Excluido = 3
}

// Interface para o filtro de produtos
export interface ProdutoFiltro {
    marcaId?: number;
    grupoId?: number;
    statusId?: number;
    generoId?: number;
} 