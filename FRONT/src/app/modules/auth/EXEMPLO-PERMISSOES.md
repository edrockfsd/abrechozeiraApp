# Guia de Implementação de Permissões

Este arquivo contém exemplos de como aplicar o sistema de permissões nas rotas da aplicação.

## Guards Disponíveis

1. **AuthGuard** - Verifica se o usuário está autenticado
2. **PermissionGuard** - Verifica roles e permissões específicas

## Exemplos de Uso

### 1. Proteger rotas com autenticação básica
```typescript
// Em qualquer arquivo de rotas (ex: produtos-routing.module.ts)
import { AuthGuard } from '../auth/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    component: ListaProdutosComponent,
    canActivate: [AuthGuard]
  }
];
```

### 2. Proteger com roles específicas
```typescript
import { PermissionGuard } from '../auth/guards/permission.guard';

const routes: Routes = [
  {
    path: 'cadastro',
    component: CadastroProdutoComponent,
    canActivate: [PermissionGuard],
    data: {
      roles: ['ADMIN', 'MANAGER']
    }
  }
];
```

### 3. Proteger com permissões específicas
```typescript
const routes: Routes = [
  {
    path: 'relatorios',
    component: RelatoriosComponent,
    canActivate: [PermissionGuard],
    data: {
      permissions: ['VIEW_REPORTS']
    }
  }
];
```

### 4. Proteger com recurso e ação específica
```typescript
const routes: Routes = [
  {
    path: 'editar/:id',
    component: EditarProdutoComponent,
    canActivate: [PermissionGuard],
    data: {
      resource: 'produtos',
      action: 'UPDATE'
    }
  }
];
```

### 5. Combinar múltiplas condições
```typescript
const routes: Routes = [
  {
    path: 'admin',
    component: AdminPanelComponent,
    canActivate: [PermissionGuard],
    data: {
      roles: ['ADMIN'],
      permissions: ['MANAGE_USERS', 'MANAGE_ROLES']
    }
  }
];
```

## Roles Padrão

- **ADMIN**: Acesso total ao sistema
- **MANAGER**: Gerenciamento de produtos, estoque e vendas
- **SELLER**: Realização de vendas e gerenciamento de pedidos
- **VIEWER**: Apenas visualização de informações

## Permissões Comuns

- **produtos:CREATE** - Criar novos produtos
- **produtos:READ** - Visualizar produtos
- **produtos:UPDATE** - Atualizar produtos
- **produtos:DELETE** - Excluir produtos
- **estoque:MANAGE** - Gerenciar estoque
- **pedidos:CREATE** - Criar pedidos
- **pedidos:READ** - Visualizar pedidos
- **lives:MANAGE** - Gerenciar lives
- **relatorios:VIEW** - Visualizar relatórios
- **usuarios:MANAGE** - Gerenciar usuários

## Como Aplicar nos Módulos Existentes

### Produtos
- **Lista**: VIEWER, SELLER, MANAGER, ADMIN
- **Cadastro**: MANAGER, ADMIN
- **Edição**: MANAGER, ADMIN
- **Exclusão**: ADMIN

### Estoque
- **Visualização**: SELLER, MANAGER, ADMIN
- **Atualização**: MANAGER, ADMIN

### Pedidos
- **Criação**: SELLER, MANAGER, ADMIN
- **Visualização**: SELLER, MANAGER, ADMIN
- **Cancelamento**: MANAGER, ADMIN

### Lives
- **Gestão**: MANAGER, ADMIN
- **Participação**: Todos os usuários autenticados

### Pessoas (Clientes)
- **Cadastro**: SELLER, MANAGER, ADMIN
- **Edição**: MANAGER, ADMIN
- **Visualização**: Todos os usuários autenticados