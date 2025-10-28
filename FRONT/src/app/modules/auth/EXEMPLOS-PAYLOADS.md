# Exemplos de Payloads - User, Role e Permission Models

## 🎯 Estrutura Completa dos Modelos

### 1. Permission Model
```json
{
  "id": "perm_001",
  "name": "produtos_create",
  "description": "Permissão para criar novos produtos",
  "resource": "produtos",
  "action": "CREATE",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00.000Z"
}
```

### 2. Role Model
```json
{
  "id": "role_001",
  "name": "MANAGER",
  "description": "Gerente com acesso a produtos, estoque e pedidos",
  "permissions": [
    {
      "id": "perm_001",
      "name": "produtos_create",
      "description": "Criar produtos",
      "resource": "produtos",
      "action": "CREATE",
      "isActive": true
    },
    {
      "id": "perm_002",
      "name": "produtos_update",
      "description": "Atualizar produtos",
      "resource": "produtos",
      "action": "UPDATE",
      "isActive": true
    },
    {
      "id": "perm_003",
      "name": "estoque_manage",
      "description": "Gerenciar estoque",
      "resource": "estoque",
      "action": "CREATE,UPDATE",
      "isActive": true
    },
    {
      "id": "perm_004",
      "name": "pedidos_create",
      "description": "Criar pedidos",
      "resource": "pedidos",
      "action": "CREATE",
      "isActive": true
    }
  ],
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00.000Z"
}
```

### 3. User Model
```json
{
  "id": "user_001",
  "email": "gerente@abrechozeira.com",
  "name": "João Silva",
  "roles": [
    {
      "id": "role_001",
      "name": "MANAGER",
      "description": "Gerente com acesso a produtos, estoque e pedidos",
      "permissions": [
        {
          "id": "perm_001",
          "name": "produtos_create",
          "description": "Criar produtos",
          "resource": "produtos",
          "action": "CREATE"
        },
        {
          "id": "perm_002",
          "name": "produtos_update",
          "description": "Atualizar produtos",
          "resource": "produtos",
          "action": "UPDATE"
        },
        {
          "id": "perm_003",
          "name": "estoque_manage",
          "description": "Gerenciar estoque",
          "resource": "estoque",
          "action": "CREATE,UPDATE"
        },
        {
          "id": "perm_004",
          "name": "pedidos_create",
          "description": "Criar pedidos",
          "resource": "pedidos",
          "action": "CREATE"
        }
      ]
    }
  ],
  "permissions": [
    "produtos_create",
    "produtos_update",
    "estoque_manage",
    "pedidos_create"
  ],
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00.000Z",
  "updatedAt": "2024-01-15T10:30:00.000Z"
}
```

## 📋 Exemplos por Tipo de Usuário

### Admin
```json
{
  "id": "user_002",
  "email": "admin@abrechozeira.com",
  "name": "Administrador",
  "roles": [
    {
      "id": "role_002",
      "name": "ADMIN",
      "description": "Acesso total ao sistema",
      "permissions": [
        {
          "id": "perm_999",
          "name": "full_access",
          "description": "Acesso completo a todos os recursos",
          "resource": "*",
          "action": "*"
        }
      ]
    }
  ],
  "permissions": ["*"],
  "isActive": true
}
```

### Vendedor
```json
{
  "id": "user_003",
  "email": "vendedor@abrechozeira.com",
  "name": "Maria Vendedora",
  "roles": [
    {
      "id": "role_003",
      "name": "SELLER",
      "description": "Vendedor com acesso a vendas e visualização",
      "permissions": [
        {
          "id": "perm_005",
          "name": "produtos_read",
          "description": "Visualizar produtos",
          "resource": "produtos",
          "action": "READ"
        },
        {
          "id": "perm_006",
          "name": "estoque_read",
          "description": "Visualizar estoque",
          "resource": "estoque",
          "action": "READ"
        },
        {
          "id": "perm_007",
          "name": "pedidos_create",
          "description": "Criar pedidos",
          "resource": "pedidos",
          "action": "CREATE"
        },
        {
          "id": "perm_008",
          "name": "clientes_create",
          "description": "Cadastrar clientes",
          "resource": "clientes",
          "action": "CREATE"
        }
      ]
    }
  ],
  "permissions": [
    "produtos_read",
    "estoque_read",
    "pedidos_create",
    "clientes_create"
  ],
  "isActive": true
}
```

### Viewer
```json
{
  "id": "user_004",
  "email": "viewer@abrechozeira.com",
  "name": "Visualizador",
  "roles": [
    {
      "id": "role_004",
      "name": "VIEWER",
      "description": "Apenas visualização de dados",
      "permissions": [
        {
          "id": "perm_009",
          "name": "produtos_read",
          "description": "Visualizar produtos",
          "resource": "produtos",
          "action": "READ"
        },
        {
          "id": "perm_010",
          "name": "estoque_read",
          "description": "Visualizar estoque",
          "resource": "estoque",
          "action": "READ"
        },
        {
          "id": "perm_011",
          "name": "relatorios_read",
          "description": "Visualizar relatórios",
          "resource": "relatorios",
          "action": "READ"
        }
      ]
    }
  ],
  "permissions": [
    "produtos_read",
    "estoque_read",
    "relatorios_read"
  ],
  "isActive": true
}
```

## 🚀 Response de Login (AuthResponse)

```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJ1c2VyXzAwMSIsImVtYWlsIjoiZ2VyZW50ZUBhYnJlY2hvemVpcmEuY29tIiwicm9sZXMiOlsiTUFOQUdFUiJdLCJpYXQiOjE3MDUzMjMwMDAsImV4cCI6MTcwNTQwOTQwMH0",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJ1c2VyXzAwMSIsInR5cGUiOiJyZWZyZXNoIiwiaWF0IjoxNzA1MzIzMDAwLCJleHAiOjE3MDU5MjMwMDB9",
  "user": {
    "id": "user_001",
    "email": "gerente@abrechozeira.com",
    "name": "João Silva",
    "roles": ["MANAGER"],
    "permissions": [
      "produtos_create",
      "produtos_update",
      "estoque_manage",
      "pedidos_create"
    ]
  }
}
```

## 📊 Permissões Comuns por Módulo

### Produtos
```json
[
  { "name": "produtos_create", "resource": "produtos", "action": "CREATE" },
  { "name": "produtos_read", "resource": "produtos", "action": "READ" },
  { "name": "produtos_update", "resource": "produtos", "action": "UPDATE" },
  { "name": "produtos_delete", "resource": "produtos", "action": "DELETE" }
]
```

### Estoque
```json
[
  { "name": "estoque_create", "resource": "estoque", "action": "CREATE" },
  { "name": "estoque_read", "resource": "estoque", "action": "READ" },
  { "name": "estoque_update", "resource": "estoque", "action": "UPDATE" },
  { "name": "estoque_delete", "resource": "estoque", "action": "DELETE" }
]
```

### Pedidos
```json
[
  { "name": "pedidos_create", "resource": "pedidos", "action": "CREATE" },
  { "name": "pedidos_read", "resource": "pedidos", "action": "READ" },
  { "name": "pedidos_update", "resource": "pedidos", "action": "UPDATE" },
  { "name": "pedidos_delete", "resource": "pedidos", "action": "DELETE" },
  { "name": "pedidos_cancel", "resource": "pedidos", "action": "CANCEL" }
]
```

### Clientes
```json
[
  { "name": "clientes_create", "resource": "clientes", "action": "CREATE" },
  { "name": "clientes_read", "resource": "clientes", "action": "READ" },
  { "name": "clientes_update", "resource": "clientes", "action": "UPDATE" },
  { "name": "clientes_delete", "resource": "clientes", "action": "DELETE" }
]
```

### Lives
```json
[
  { "name": "lives_create", "resource": "lives", "action": "CREATE" },
  { "name": "lives_read", "resource": "lives", "action": "READ" },
  { "name": "lives_update", "resource": "lives", "action": "UPDATE" },
  { "name": "lives_delete", "resource": "lives", "action": "DELETE" },
  { "name": "lives_start", "resource": "lives", "action": "START" },
  { "name": "lives_end", "resource": "lives", "action": "END" }
]
```

## 📝 Notas Importantes

1. **IDs**: Use UUID ou auto-increment conforme sua preferência de banco de dados
2. **Timestamps**: Sempre use ISO 8601 format (YYYY-MM-DDTHH:mm:ss.sssZ)
3. **Actions**: Pode ser uma string única ou múltiplas separadas por vírgula
4. **Resource**: Use nomes em minúsculo e no plural
5. **Cache**: Considere cachear as permissões no frontend para melhor performance
6. **Soft Delete**: Adicione `deletedAt` se usar soft delete no banco