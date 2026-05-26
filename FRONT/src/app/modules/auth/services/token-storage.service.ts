import { Injectable } from '@angular/core';

/**
 * Serviço responsável pelo armazenamento e gerenciamento de tokens de autenticação
 */
@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private readonly TOKEN_KEY = 'auth-token';
  private readonly REFRESH_TOKEN_KEY = 'auth-refresh-token';
  private readonly USER_KEY = 'auth-user';

  constructor() { }

  /**
   * Limpa todos os dados de autenticação do storage
   */
  public clear(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  /**
   * Salva o token JWT no storage
   * @param token Token JWT a ser armazenado
   */
  public saveToken(token: string): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  /**
   * Recupera o token JWT do storage
   * @returns Token JWT armazenado ou null se não existir
   */
  public getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  /**
   * Salva o refresh token no storage
   * @param refreshToken Refresh token a ser armazenado
   */
  public saveRefreshToken(refreshToken: string): void {
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  /**
   * Recupera o refresh token do storage
   * @returns Refresh token armazenado ou null se não existir
   */
  public getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  /**
   * Salva os dados do usuário no storage
   * @param user Objeto com os dados do usuário
   */
  public saveUser(user: any): void {
    localStorage.removeItem(this.USER_KEY);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  /**
   * Recupera os dados do usuário do storage
   * @returns Objeto com os dados do usuário ou null se não existir
   */
  public getUser(): any {
    const user = localStorage.getItem(this.USER_KEY);
    if (user) {
      return JSON.parse(user);
    }
    return null;
  }
}