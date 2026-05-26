import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { User, UserLogin, AuthResponse } from '../models/user.model';
import { RoleName } from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class MockAuthService {
  private mockUsers: User[] = [
    {
      id: '1',
      email: 'admin@abrechozeira.com',
      name: 'Administrador',
      roles: [
        {
          id: '1',
          name: RoleName.ADMIN,
          description: 'Administrador do Sistema',
          permissions: [
            { id: '1', name: 'FULL_ACCESS', description: 'Acesso completo ao sistema', resource: '*', action: '*' }
          ],
          isActive: true,
          createdAt: new Date()
        }
      ],
      permissions: ['FULL_ACCESS'],
      isActive: true,
      createdAt: new Date(),
      updatedAt: new Date()
    },
    {
      id: '2',
      email: 'gerente@abrechozeira.com',
      name: 'Gerente',
      roles: [
        {
          id: '2',
          name: RoleName.MANAGER,
          description: 'Gerente',
          permissions: [
            { id: '2', name: 'MANAGE_PRODUCTS', description: 'Gerenciar produtos', resource: 'produtos', action: 'CREATE,UPDATE,DELETE' },
            { id: '3', name: 'MANAGE_INVENTORY', description: 'Gerenciar estoque', resource: 'estoque', action: 'CREATE,UPDATE' },
            { id: '4', name: 'MANAGE_ORDERS', description: 'Gerenciar pedidos', resource: 'pedidos', action: 'CREATE,UPDATE' },
            { id: '5', name: 'MANAGE_LIVES', description: 'Gerenciar lives', resource: 'lives', action: 'CREATE,UPDATE' }
          ],
          isActive: true,
          createdAt: new Date()
        }
      ],
      permissions: ['MANAGE_PRODUCTS', 'MANAGE_INVENTORY', 'MANAGE_ORDERS', 'MANAGE_LIVES'],
      isActive: true,
      createdAt: new Date(),
      updatedAt: new Date()
    },
    {
      id: '3',
      email: 'vendedor@abrechozeira.com',
      name: 'Vendedor',
      roles: [
        {
          id: '3',
          name: RoleName.SELLER,
          description: 'Vendedor',
          permissions: [
            { id: '6', name: 'VIEW_PRODUCTS', description: 'Visualizar produtos', resource: 'produtos', action: 'READ' },
            { id: '7', name: 'CREATE_ORDERS', description: 'Criar pedidos', resource: 'pedidos', action: 'CREATE' },
            { id: '8', name: 'VIEW_INVENTORY', description: 'Visualizar estoque', resource: 'estoque', action: 'READ' }
          ],
          isActive: true,
          createdAt: new Date()
        }
      ],
      permissions: ['VIEW_PRODUCTS', 'CREATE_ORDERS', 'VIEW_INVENTORY'],
      isActive: true,
      createdAt: new Date(),
      updatedAt: new Date()
    },
    {
      id: '4',
      email: 'visualizador@abrechozeira.com',
      name: 'Visualizador',
      roles: [
        {
          id: '4',
          name: RoleName.VIEWER,
          description: 'Visualizador',
          permissions: [
            { id: '9', name: 'VIEW_PRODUCTS', description: 'Visualizar produtos', resource: 'produtos', action: 'READ' },
            { id: '10', name: 'VIEW_INVENTORY', description: 'Visualizar estoque', resource: 'estoque', action: 'READ' },
            { id: '11', name: 'VIEW_ORDERS', description: 'Visualizar pedidos', resource: 'pedidos', action: 'READ' }
          ],
          isActive: true,
          createdAt: new Date()
        }
      ],
      permissions: ['VIEW_PRODUCTS', 'VIEW_INVENTORY', 'VIEW_ORDERS'],
      isActive: true,
      createdAt: new Date(),
      updatedAt: new Date()
    }
  ];

  private mockToken = 'mock-jwt-token-12345';

  login(credentials: UserLogin): Observable<AuthResponse> {
    const user = this.mockUsers.find(u => u.email === credentials.email);
    
    if (!user) {
      throw new Error('Usuário não encontrado');
    }

    if (credentials.password !== '123456') {
      throw new Error('Senha incorreta');
    }

    return of({
      user: user,
      token: this.mockToken,
      refreshToken: 'mock-refresh-token-12345'
    }).pipe(delay(1000));
  }

  getCurrentUser(): Observable<User | null> {
    const token = localStorage.getItem('token');
    if (!token) return of(null).pipe(delay(500));

    const user = this.mockUsers[0]; // Simula usuário logado
    return of(user).pipe(delay(500));
  }

  validateToken(token: string): Observable<boolean> {
    return of(token === this.mockToken).pipe(delay(500));
  }
}