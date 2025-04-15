import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Produto, ProdutoFiltro } from '../models/produto';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProdutoService {
  private apiUrl = `${environment.apiUrl}/Produtos`;

  constructor(private http: HttpClient) { }

  private formatarData(data: Date): string {
    return data.toISOString().slice(0, 19);
  }

  listar(filtro?: ProdutoFiltro): Observable<Produto[]> {
    let params = new HttpParams();
    
    if (filtro) {
      if (filtro.marcaId) params = params.set('marcaId', filtro.marcaId.toString());
      if (filtro.grupoId) params = params.set('grupoId', filtro.grupoId.toString());
      if (filtro.statusId) params = params.set('statusId', filtro.statusId.toString());
      if (filtro.generoId) params = params.set('generoId', filtro.generoId.toString());
    }

    return this.http.get<Produto[]>(this.apiUrl, { params });
  }

  buscarPorId(id: number): Observable<Produto> {    
    return this.http.get<Produto>(`${this.apiUrl}/${id}`);
  }

  criar(produto: Produto): Observable<Produto> {
    produto.dataCompra = new Date(produto.dataCompra);
    produto.dataAlteracao = new Date();
    produto.usuarioModificacaoId = 1; // TODO: Pegar o ID do usuário logado
    
    // Formatando as datas
    if (produto.dataCompra) {
      produto.dataCompra = this.formatarData(produto.dataCompra) as any;
    }
    if (produto.dataAlteracao) {
      produto.dataAlteracao = this.formatarData(produto.dataAlteracao) as any;
    }
    
    // Removendo campos undefined ou null
    Object.keys(produto).forEach(key => {
      if (produto[key] === undefined || produto[key] === null) {
        delete produto[key];
      }
    });

    return this.http.post<Produto>(this.apiUrl, produto);
  }

  atualizar(id: number, produto: Produto): Observable<Produto> {
    produto.dataAlteracao = new Date();
    produto.usuarioModificacaoId = 1; // TODO: Pegar o ID do usuário logado
    
    // Formatando as datas
    if (produto.dataCompra) {
      produto.dataCompra = this.formatarData(new Date(produto.dataCompra)) as any;
    }
    if (produto.dataAlteracao) {
      produto.dataAlteracao = this.formatarData(produto.dataAlteracao) as any;
    }
    
    // Removendo campos undefined ou null
    Object.keys(produto).forEach(key => {
      if (produto[key] === undefined || produto[key] === null) {
        delete produto[key];
      }
    });

    return this.http.put<Produto>(`${this.apiUrl}/${id}`, produto);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  buscarPorGrupo(grupoId: number): Observable<Produto[]> {
    return this.http.get<Produto[]>(`${this.apiUrl}/grupo/${grupoId}`);
  }

  buscarPorMarca(marcaId: number): Observable<Produto[]> {
    return this.http.get<Produto[]>(`${this.apiUrl}/marca/${marcaId}`);
  }

  buscarPorStatus(statusId: number): Observable<Produto[]> {
    return this.http.get<Produto[]>(`${this.apiUrl}/status/${statusId}`);
  }

  buscarPorGenero(generoId: number): Observable<Produto[]> {
    return this.http.get<Produto[]>(`${this.apiUrl}/genero/${generoId}`);
  }

  buscarPorCodigoEstoque(codigoEstoque: string): Observable<Produto> {
    return this.http.get<Produto>(`${this.apiUrl}/GetProdutoByCodigoEstoque?codigoEstoque=${codigoEstoque}`);
  }
} 