import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of, forkJoin, switchMap } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface User {
  id: number;
  username: string;
  email: string;
  pessoaId: number;
  firstName: string;
  lastName: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  userRoles?: UserRole[];
  userPermissions?: UserPermission[];
}

export interface UserRole {
  id: number;
  userId: number;
  roleId: number;
  role: Role;
}

export interface Role {
  id: number;
  name: string;
  description: string;
  rolePermissions?: RolePermission[];
  permissions?: Permission[]; // Adicionado para compatibilidade com o backend
}

// Enum mapping for RoleType values
export const RoleTypeMap = {
  0: 'ADMIN',
  1: 'MANAGER', 
  2: 'SELLER',
  3: 'VIEWER'
} as const;

export type RoleType = typeof RoleTypeMap[keyof typeof RoleTypeMap];

export interface RolePermission {
  id: number;
  roleId: number;
  permissionId: number;
  permission: Permission;
}

export interface Permission {
  id: number;
  name: string;
  description: string;
}

export interface UserPermission {
  id: number;
  userId: number;
  permissionId: number;
  permission: Permission;
}

export interface CreateUserRequest {
  username: string;
  email: string;
  pessoaId: number;
  password: string;
  isActive: boolean;
  roleIds: number[];
  permissionIds?: number[];
}

export interface UpdateUserRequest {
  username?: string;
  email?: string;
  password?: string;
  isActive?: boolean;
  roleIds?: number[];
  permissionIds?: number[];
}

export interface CreateRoleRequest {
  name: string;
  description: string;
  isActive: boolean;
}

export interface UpdateRoleRequest {
  name?: string;
  description?: string;
  isActive?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) { }

  getUsers(): Observable<User[]> {
    return this.http.get<any[]>(this.apiUrl).pipe(
      map(users => {
        return users.map(user => {
          // Mapear os dados para a estrutura esperada
          const mappedUser: User = {
            id: user.id,
            pessoaId: (user.pessoa?.id ?? user.Pessoa?.Id) ?? 0,
            username: user.Name || user.name || user.username || '',
            email: user.email || user.Email || '',
            firstName: user.nome || user.Nome ||  '',
            lastName: user.sobrenome || user.Sobrenome || '',
            isActive: (user.isActive ?? user.IsActive ?? user.active) ?? false,
            createdAt: user.createdAt || user.CreatedAt || '',
            updatedAt: user.updatedAt || user.UpdatedAt || '',
            userRoles: user.userRoles || user.UserRoles || [],
            userPermissions: user.userPermissions || user.UserPermissions || []
          };
          
          // Processar roles se existirem
          if (mappedUser.userRoles) {
            mappedUser.userRoles = mappedUser.userRoles.map(userRole => ({
              ...userRole,
              role: {
                ...userRole.role,
                name: this.convertRoleName(userRole.role.name)
              }
            }));
          }

          // Se API devolveu Roles (sem userRoles), converter para userRoles
          const apiRoles = (user as any).Roles || (user as any).roles;
          if ((!mappedUser.userRoles || mappedUser.userRoles.length === 0) && Array.isArray(apiRoles)) {
            mappedUser.userRoles = apiRoles.map((r: any, idx: number) => ({
              id: r.id ?? r.Id ?? idx,
              userId: mappedUser.id,
              roleId: r.id ?? r.Id,
              role: {
                id: r.id ?? r.Id,
                name: this.convertRoleName(r.name ?? r.Name),
                description: r.description ?? r.Description,
                rolePermissions: [],
                permissions: (r.permissions ?? r.Permissions) ?? []
              }
            }));
          }

          return mappedUser;
        });
      })
    );
  }

  getUser(id: number): Observable<User> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(user => {
        // Mapear os dados para a estrutura esperada
        const mappedUser: User = {
          id: user.id,
          pessoaId: (user.pessoa?.id ?? user.Pessoa?.Id) ?? 0,
          username: user.userName || user.username || user.UserName || user.Name || user.name || '',
          email: user.email || user.Email || '',
          firstName: user.nome || user.Nome || '',
          lastName: user.sobrenome || user.Sobrenome || '',
          isActive: (user.isActive ?? user.IsActive ?? user.active) ?? false,
          createdAt: user.createdAt || user.CreatedAt || '',
          updatedAt: user.updatedAt || user.UpdatedAt || '',
          userRoles: user.userRoles || user.UserRoles || [],
          userPermissions: user.userPermissions || user.UserPermissions || []
        };
        // Se vier Roles sem userRoles, converter
        const apiRoles = (user as any).Roles || (user as any).roles;
        if ((!mappedUser.userRoles || mappedUser.userRoles.length === 0) && Array.isArray(apiRoles)) {
          mappedUser.userRoles = apiRoles.map((r: any, idx: number) => ({
            id: r.id ?? r.Id ?? idx,
            userId: mappedUser.id,
            roleId: r.id ?? r.Id,
            role: {
              id: r.id ?? r.Id,
              name: this.convertRoleName(r.name ?? r.Name),
              description: r.description ?? r.Description,
              rolePermissions: [],
              permissions: (r.permissions ?? r.Permissions) ?? []
            }
          }));
        }
        return mappedUser;
      })
    );
  }

  createUser(user: CreateUserRequest): Observable<User> {
    const payload = {
      email: user.email,
      name: user.username,
      password: user.password,
      isActive: user.isActive,
      pessoaId: user.pessoaId
    };

    return this.http.post<User>(this.apiUrl, payload).pipe(
      switchMap((created) => {
        const roleIds = user.roleIds || [];
        if (roleIds.length === 0) {
          return of(created);
        }
        const addRoleCalls = roleIds.map(roleId =>
          this.http.post<void>(`${this.apiUrl}/${created.id}/roles/${roleId}`, {})
        );
        return forkJoin(addRoleCalls).pipe(map(() => created));
      })
    );
  }

  updateUser(id: number, user: UpdateUserRequest): Observable<User> {
    const payload: any = {
      email: user.email,
      name: user.username,
      isActive: user.isActive
    };
    if (user.password) {
      payload.password = user.password;
    }

    return this.http.put<void>(`${this.apiUrl}/${id}`, payload).pipe(
      switchMap(() => {
        // Opcional: sincronizar roles se fornecido
        if (!user.roleIds) {
          return this.getUser(id);
        }
        return this.getUser(id).pipe(
          switchMap(current => {
            const currentRoleIds = (current.userRoles || []).map(r => r.roleId);
            const desired = new Set(user.roleIds as number[]);
            const toAdd = user.roleIds!.filter(rid => !currentRoleIds.includes(rid));
            const toRemove = currentRoleIds.filter(rid => !desired.has(rid));

            const calls: Observable<void>[] = [];
            toAdd.forEach(roleId => {
              calls.push(this.http.post<void>(`${this.apiUrl}/${id}/roles/${roleId}`, {}));
            });
            toRemove.forEach(roleId => {
              calls.push(this.http.delete<void>(`${this.apiUrl}/${id}/roles/${roleId}`));
            });

            if (calls.length === 0) {
              return of(current);
            }
            return forkJoin(calls).pipe(switchMap(() => this.getUser(id)));
          })
        );
      })
    );
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getRoles(): Observable<Role[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/roles`).pipe(
      map(roles => {
        return roles.map(role => {
          // Mapear os dados para a estrutura esperada
          const mappedRole: Role = {
            id: role.id,
            name: role.name || role.Name || '',
            description: role.description || role.Description || '',
            rolePermissions: role.rolePermissions || role.RolePermissions || [],
            permissions: role.permissions || role.Permissions || []
          };
          return {
            ...mappedRole,
            name: this.convertRoleName(mappedRole.name)
          };
        });
      })
    );
  }

  private convertRoleName(name: string | number): string {
    if (typeof name === 'number') {
      return RoleTypeMap[name as keyof typeof RoleTypeMap] || 'UNKNOWN';
    }
    return name;
  }

  getRole(id: number): Observable<Role> {
    return this.http.get<Role>(`${environment.apiUrl}/roles/${id}`).pipe(
      map(role => ({
        ...role,
        name: this.convertRoleName(role.name)
      }))
    );
  }

  createRole(role: CreateRoleRequest): Observable<Role> {
    return this.http.post<Role>(`${environment.apiUrl}/roles`, role);
  }

  updateRole(id: number, role: UpdateRoleRequest): Observable<Role> {
    return this.http.put<Role>(`${environment.apiUrl}/roles/${id}`, role);
  }

  deleteRole(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/roles/${id}`);
  }

  addPermissionToRole(roleId: number, permissionId: number): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/roles/${roleId}/permissions/${permissionId}`, {});
  }

  removePermissionFromRole(roleId: number, permissionId: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/roles/${roleId}/permissions/${permissionId}`);
  }

  getPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${environment.apiUrl}/permissions`);
  }
}
