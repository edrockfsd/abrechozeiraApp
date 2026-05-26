import { Role } from './role.model';

export interface User {
  id: string;
  email: string;
  name: string;
  roles: Role[];
  permissions: string[];
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface UserLogin {
  email: string;
  password: string;
}

export interface AuthResponse {
  user: User;
  token: string;
  refreshToken: string;
}