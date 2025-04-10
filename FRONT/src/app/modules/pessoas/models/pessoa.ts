export interface Pessoa {
    id: number;
    nome: string;
    cpf: string;
    rg: string;
    dataNascimento: string;
    observacao: string;
    usuarioCriacaoId: number;
    usuarioModificacaoId: number;
    email: string;
    telefone: string;
    pessoaGeneroId: number;
    pessoaCategoriaId: number;
    pessoaTipoId: number;
    statusId: number;
    nickName: string;
}

export interface PessoaCreate {
    nome: string;
    cpf: string;
    rg: string;
    dataNascimento: string;
    observacao: string;
    email: string;
    telefone: string;
    pessoaGeneroId: number;
    pessoaCategoriaId: number;
    pessoaTipoId: number;
    statusId: number;
    nickName: string;
}

export enum PessoaStatus {
    Ativo = 1,
    Inativo = 2,
    Excluido = 3
}

export interface PessoaCategoria {
    id: number;
    descricao: string;
}

export interface PessoaTipo {
    id: number;
    descricao: string;
} 