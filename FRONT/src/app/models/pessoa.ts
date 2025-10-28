
export interface Pessoa {
  id: number;
  nome: string;
  dataNascimento?: Date;
  email?: string;
  telefone?: string;
  pessoaGeneroId: number;
  cpf?: string;
  rg?: string;
  nickName?: string;
  observacoes?: string;
  dataInclusao?: Date;
  pessoaCategoriaId: number;
  pessoaTipoId: number;
  statusId: number;
}
