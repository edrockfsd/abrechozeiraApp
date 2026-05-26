import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { User } from '../models/user.model';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private mockUsers: User[] = [
    {
      id: '1',
      email: 'admin@abrechozeira.com',
      name: 'Administrador',
      roles: [
        {
          id: '1',
          name: 'ADMIN',
          description: 'Administrador do Sistema',
          permissions: [],
          isActive: true,
          createdAt: new Date()
        }
      ],
      isActive: true,
      createdAt: new Date()
    },
    {
      id: '2',
      email: 'gerente@abrechozeira.com',
      name: 'Gerente Silva',
      roles: [
        {
          id: '2',
          name: 'MANAGER',
          description: 'Gerente',
          permissions: [],
          isActive: true,
          createdAt: new Date()
        }
      ],
      isActive: true,
      createdAt: new Date()
    }
  ];

  constructor(private http: HttpClient) {}

  getUsers(): Observable<User[]> {
    // Mock - substituir com chamada real ao backend
    return of(this.mockUsers).pipe(delay(1000));
  }

  getUserById(id: string): Observable<User | null> {
    // Mock - substituir com chamada real ao backend
    const user = this.mockUsers.find(u => u.id === id);
    return of(user || null).pipe(delay(500));
  }

  createUser(user: Partial<User>): Observable<User> {
    // Mock - substituir com chamada real ao backend
    const newUser: User = {
      id: String(this.mockUsers.length + 1),
      email: user.email || '',
      name: user.name || '',
      roles: user.roles || [],
      isActive: true,
      createdAt: new Date()
    };
    this.mockUsers.push(newUser);
    return of(newUser).pipe(delay(1000));
  }

  updateUser(id: string, user: Partial<User>): Observable<User> {
    // Mock - substituir com chamada real ao backend
    const index = this.mockUsers.findIndex(u => u.id === id);
    if (index !== -1) {
      this.mockUsers[index] = { ...this.mockUsers[index], ...user };
      return of(this.mockUsers[index]).pipe(delay(1000));
    }
    throw new Error('Usuário não encontrado');
  }

  deleteUser(id: string): Observable<boolean> {
    // Mock - substituir com chamada real ao backend
    const index = this.mockUsers.findIndex(u => u.id === id);
    if (index !== -1) {
      this.mockUsers.splice(index, 1);
      return of(true).pipe(delay(1000));
    }
    return of(false).pipe(delay(1000));
  }
}