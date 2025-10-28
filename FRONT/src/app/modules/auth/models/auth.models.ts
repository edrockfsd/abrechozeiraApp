/**
 * Modelos de dados para autenticação
 */

// Interface para requisição de login
export interface AuthRequest {
  email: string;
  password: string;
}

// Interface para resposta de autenticação
export interface AuthResponse {
  token: string;
  refreshToken?: string;
  expiresIn?: number;
  user: User;
}

// Interface para usuário
export interface User {
  id: string;
  name: string;
  email: string;
  roles: Role[];
  isActive?: boolean;
  lastLogin?: Date;
  createdAt?: Date;
  updatedAt?: Date;
}

// Interface para papel/função do usuário
export interface Role {
  id: string;
  name: string;
  description?: string;
  permissions: Permission[];
}

// Interface para permissão
export interface Permission {
  id: string;
  resource: string;
  action: string;
  description?: string;
}