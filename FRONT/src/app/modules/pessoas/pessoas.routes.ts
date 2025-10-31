import { Routes } from '@angular/router';
import { ListaPessoasComponent } from './pages/lista-pessoas/lista-pessoas.component';
import { CadastroPessoaComponent } from './pages/cadastro-pessoa/cadastro-pessoa.component';
import { CadastroEnderecoComponent } from './pages/cadastro-endereco/cadastro-endereco.component';
import { ListaEnderecosComponent } from './pages/lista-enderecos/lista-enderecos.component';
import { AuthGuard } from '../auth/guards/auth.guard';
import { PermissionGuard } from '../auth/guards/permission.guard';

export const PESSOAS_ROUTES: Routes = [
  {
    path: '',
    component: ListaPessoasComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['clientes_read', 'full_access'] }
  },
  {
    path: 'novo',
    component: CadastroPessoaComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['clientes_create', 'full_access'] }
  },
  {
    path: 'editar/:id',
    component: CadastroPessoaComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['clientes_create', 'full_access'] }
  },
  {
    path: ':pessoaId/endereco',
    component: ListaEnderecosComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['clientes_read', 'full_access'] }
  },
  {
    path: ':pessoaId/endereco/novo',
    component: CadastroEnderecoComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['clientes_create', 'full_access'] }
  },
  {
    path: ':pessoaId/endereco/editar/:id',
    component: CadastroEnderecoComponent,
    canActivate: [AuthGuard, PermissionGuard],
    data: { permissions: ['clientes_create', 'full_access'] }
  }
];
