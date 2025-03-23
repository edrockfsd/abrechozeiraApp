export interface Pessoa {
    id: number;
    nome: string;
    dataNascimento: Date;
    email: string;
    telefone: string;
    sexo: string;
    pessoaCategoriaId: number;
    pessoaTipoId: number;
    nickName: string;
    dataInclusao: Date;
    statusId: number;
}

export interface PessoaCreate {
    nome: string;
    dataNascimento: Date;
    email: string;
    telefone: string;
    sexo: string;
    pessoaCategoriaId: number;
    pessoaTipoId: number;
    nickName: string;
    statusId: number;
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