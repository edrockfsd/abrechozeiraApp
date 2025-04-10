import { Routes } from '@angular/router';
import { ListaPessoasComponent } from './pages/lista-pessoas/lista-pessoas.component';
import { CadastroPessoaComponent } from './pages/cadastro-pessoa/cadastro-pessoa.component';
import { CadastroEnderecoComponent } from './pages/cadastro-endereco/cadastro-endereco.component';
import { ListaEnderecosComponent } from './pages/lista-enderecos/lista-enderecos.component';

export const PESSOAS_ROUTES: Routes = [
  {
    path: '',
    component: ListaPessoasComponent
  },
  {
    path: 'novo',
    component: CadastroPessoaComponent
  },
  {
    path: 'editar/:id',
    component: CadastroPessoaComponent
  },
  {
    path: ':pessoaId/endereco',
    component: ListaEnderecosComponent
  },
  {
    path: ':pessoaId/endereco/novo',
    component: CadastroEnderecoComponent
  },
  {
    path: ':pessoaId/endereco/editar/:id',
    component: CadastroEnderecoComponent
  }
];