import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { Estoque, EstoqueCreate } from '../models/estoque';

@Injectable({
  providedIn: 'root'
})
export class EstoqueService {
  private apiUrl = 'https://localhost:7194/api/Estoque';
  private apiUrlCompleto = 'https://localhost:7194/api/Estoque/GetEstoquesCompleto';
  private apiUrlBuscaCodigo = 'https://localhost:7194/api/Estoque/GetEstoqueByCodigoEstoque';
  private apiUrlUltimoCodigo = 'https://localhost:7194/api/Estoque/GetLastCodigoEstoque';

  constructor(private http: HttpClient) { }

  listar(): Observable<Estoque[]> {
    return this.http.get<Estoque[]>(this.apiUrlCompleto);
  }

  buscarPorId(id: number): Observable<Estoque> {
    return this.http.get<Estoque>(`${this.apiUrl}/${id}`);
  }

  buscarPorCodigo(codigoEstoque: string): Observable<Estoque> {
    return this.http.get<Estoque>(`${this.apiUrlBuscaCodigo}?codigoEstoque=${codigoEstoque}`);
  }

  obterUltimoCodigoEstoque(): Observable<string> {
    return this.http.get<string>(this.apiUrlUltimoCodigo).pipe(
      map(codigo => {
        if (!codigo) {
          return '1';
        }
        const proximoCodigo = (parseInt(codigo) + 1).toString();
        return proximoCodigo;
      }),
      catchError(erro => {
        console.error('Erro ao obter último código de estoque:', erro);
        return throwError(erro);
      })
    );
  }

  criar(estoque: EstoqueCreate): Observable<Estoque> {
    return this.http.post<Estoque>(this.apiUrl, estoque);
  }

  atualizar(id: number, estoque: Estoque): Observable<Estoque> {
    return this.http.put<Estoque>(`${this.apiUrl}/${id}`, estoque);
  }

  excluir(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  criarEstoqueInicial(produtoId: number): Observable<Estoque> {
    return this.obterUltimoCodigoEstoque().pipe(
      switchMap(novoCodigo => {
        const novoEstoque: EstoqueCreate = {
          codigoEstoque: novoCodigo,
          quantidade: 1,
          localizacao: '',
          produtoId: produtoId,
          usuarioModificacaoId: 1
        };

        return this.criar(novoEstoque);
      }),
      catchError(erro => {
        console.error('Erro ao criar estoque inicial:', erro);
        return throwError(erro);
      })
    );
  }
} 