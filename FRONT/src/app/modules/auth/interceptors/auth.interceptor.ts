import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private refreshInProgress = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private authService: AuthService, private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();

    const isAuthEndpoint = /\/auth\/(login|refresh-token)$/i.test(req.url);

    const addAuthHeader = (request: HttpRequest<any>, jwt: string | null) =>
      jwt ? request.clone({ headers: request.headers.set('Authorization', `Bearer ${jwt}`) }) : request;

    const authReq = token && !isAuthEndpoint ? addAuthHeader(req, token) : req;

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        // 403: sem permissão -> envia para Acesso Negado
        if (error.status === 403) {
          const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
          this.router.navigate(['/auth/acesso-negado'], { queryParams: { returnUrl } });
          return throwError(() => error);
        }
        // 401 em endpoint não-auth: tenta refresh
        if (error.status !== 401 || isAuthEndpoint) {
          return throwError(() => error);
        }
        // 401 em rota protegida: tenta refresh
        if (!this.refreshInProgress) {
          this.refreshInProgress = true;
          this.refreshTokenSubject.next(null);

          return this.authService.refreshToken().pipe(
            switchMap((newToken: string) => {
              this.refreshInProgress = false;
              this.refreshTokenSubject.next(newToken);
              const retryReq = addAuthHeader(req, newToken);
              return next.handle(retryReq);
            }),
            catchError(refreshErr => {
              this.refreshInProgress = false;
              // Redirect to login on refresh failure
              this.authService.logout();
              const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
              this.router.navigate(['/auth/login'], { queryParams: { returnUrl } });
              return throwError(() => refreshErr);
            })
          );
        } else {
          // Aguarda o refresh concluir e repete a requisição
          return this.refreshTokenSubject.pipe(
            filter(t => t != null),
            take(1),
            switchMap((newToken) => {
              const retryReq = addAuthHeader(req, newToken);
              return next.handle(retryReq);
            })
          );
        }
      })
    );
  }
}
