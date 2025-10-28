import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class PermissionGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    const requiredRoles = route.data['roles'] as string[];
    const requiredPermissions = route.data['permissions'] as string[];
    const requiredResource = route.data['resource'] as string;
    const requiredAction = route.data['action'] as string;

    return this.authService.currentUser$.pipe(
      take(1),
      map(user => {
        if (!user) {
          this.router.navigate(['/auth/login']);
          return false;
        }

        // Verificar roles necessárias
        if (requiredRoles && requiredRoles.length > 0) {
          const hasRequiredRole = requiredRoles.some(role => 
            this.authService.hasRole(role)
          );
          if (!hasRequiredRole) {
            this.router.navigate(['/acesso-negado']);
            return false;
          }
        }

        // Verificar permissões específicas
        if (requiredPermissions && requiredPermissions.length > 0) {
          const hasRequiredPermission = requiredPermissions.some(permission => 
            this.authService.hasPermission(permission)
          );
          if (!hasRequiredPermission) {
            this.router.navigate(['/acesso-negado']);
            return false;
          }
        }

        // Verificar permissão para recurso/ação específica
        if (requiredResource && requiredAction) {
          if (!this.authService.hasPermissionFor(requiredResource, requiredAction)) {
            this.router.navigate(['/acesso-negado']);
            return false;
          }
        }

        return true;
      })
    );
  }
}