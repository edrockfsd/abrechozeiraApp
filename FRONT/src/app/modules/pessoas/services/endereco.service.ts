import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { tap } from 'rxjs/operators';

export interface Endereco {
  id?: number;
  pessoaId: number;
  tipoEnderecoId: number;
  cep: string;
  logradouro: string;
  unidade: string;
  complemento?: string;
  bairro: string;
  localidade: string;
  codigoLocalidadeIBGE?: string;
  estado: string;
  observacoes?: string;
  usuarioCriacaoId?: number;
  usuarioModificacaoId?: number;
  dataCriacao?: Date;
  dataModificacao?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class EnderecoService {
  private apiUrl = `${environment.apiUrl}/Endereco`;

  constructor(private http: HttpClient) { }

  listarPorPessoa(pessoaId: number): Observable<Endereco[]> {
    return this.http.get<Endereco[]>(`${this.apiUrl}/GetEnderecoPorPessoa?pessoaID=${pessoaId}`);
  }

  criar(endereco: Endereco): Observable<Endereco> {
    return this.http.post<Endereco>(this.apiUrl, endereco);
  }

  atualizar(id: number, endereco: Endereco): Observable<Endereco> {
    return this.http.put<Endereco>(`${this.apiUrl}/${id}`, endereco);
  }

  obterPorId(id: number): Observable<Endereco> {
    return this.http.get<Endereco>(`${this.apiUrl}/${id}`);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
} 