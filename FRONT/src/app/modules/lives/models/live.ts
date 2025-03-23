export interface Live {
  id: number;
  titulo: string;
  observacoes: string;
  dataLive: string;
  dataAlteracao: string;
  usuarioModificacaoId: number;
  usuarioModificacao: string;
}

export interface LiveCreate {
  titulo: string;
  observacoes: string;
  dataLive: string;
  usuarioModificacaoId: number;
} 