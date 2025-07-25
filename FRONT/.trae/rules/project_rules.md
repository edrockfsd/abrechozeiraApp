Fase 1: Convenções de Nomenclatura e Organização de Código
Concluído: Início da Fase 1. Próximos Passos: Detalhar as convenções de nomenclatura e diretrizes de organização para arquivos e funções. Fases Restantes: 2, 3, 4, 5, 6.

1. Convenções de Nomenclatura
A consistência na nomenclatura é crucial para a legibilidade e manutenção do código, especialmente para uma equipe júnior.
1.1. Arquivos e Pastas
Geral: Utilize kebab-case (tudo em minúsculas e palavras separadas por hífen).
Exemplos: cadastro-produto.component.ts, produto.service.ts, lista-produtos.component.html, app.component.scss.
Módulos: O nome do arquivo do módulo deve ser [nome-do-modulo].module.ts (ex: produtos.module.ts).
Rotas: O nome do arquivo de rotas deve ser [nome-do-modulo]-routing.module.ts (ex: produtos-routing.module.ts).
Serviços Globais: Serviços no diretório src/app/services devem seguir o padrão [nome-do-servico].service.ts (ex: toast.service.ts).
1.2. Classes e Interfaces
Classes: Utilize PascalCase (primeira letra de cada palavra em maiúscula).
Componentes: [NomeDoComponente]Component (ex: CadastroProdutoComponent, ListaProdutosComponent).
Serviços: [NomeDoServico]Service (ex: ProdutoService, ToastService).
Módulos: [NomeDoModulo]Module (ex: ProdutosModule, SharedModule).
Modelos/Entidades: [NomeDoModelo] (ex: Produto, ProdutoGrupo).
Enums: [NomeDoEnum] (ex: ProdutoStatus).
Interfaces: Utilize PascalCase e, opcionalmente, prefixe com I para clareza, mas não é estritamente necessário se o contexto for claro.
Exemplos: Produto (preferencialmente, como já utilizado), ou IProduto. ProdutoFiltro.
1.3. Variáveis e Propriedades
Geral: Utilize camelCase (primeira letra minúscula, primeira letra das palavras seguintes em maiúscula).
Exemplos: produtoForm, isEdicao, produtoId, salvando.
Variáveis com o sinal $: Para Observables, é uma boa prática adicionar um $ no final do nome da variável para indicar que é um Observable (ex: dados$).
Constantes (globais ou imutáveis): Utilize UPPER_SNAKE_CASE (tudo em maiúsculas, palavras separadas por underscore).
Exemplo: API_URL, TIMEOUT_PADRAO.
1.4. Métodos/Funções
Geral: Utilize camelCase.
Exemplos: ngOnInit(), criarFormulario(), carregarDados(), onSubmit(), showSuccess().
Métodos de Ciclo de Vida do Angular: Mantenha a nomenclatura padrão do Angular (ex: ngOnInit, ngAfterViewInit).
Manipuladores de Eventos: Prefixar com on (ex: onClick, onCancel).
Métodos Privados: É uma boa prática prefixar métodos privados com _ para indicar sua visibilidade interna, embora o TypeScript já garanta isso. No entanto, o Angular Style Guide não recomenda explicitamente. Para simplicidade com uma equipe júnior, podemos omitir o _ e confiar na palavra-chave private do TypeScript.


Fase 2: Padrões de Design e Melhores Práticas de Componentes/Serviços

Concluído: Compilação da Fase 1 e início da Fase 2.
Próximos Passos: Detalhar os padrões de design, comunicação de componentes, uso de BehaviorSubjects e formulários reativos.
Fases Restantes: 3, 4, 5, 6.

3. Padrões de Design e Melhores Práticas de Componentes/Serviços


3.1. Criação e Uso de Componentes

Componentes de Apresentação (Dumb/Pure Components):
Focam na UI e em como os dados são apresentados.
Recebem dados via @Input().
Emitem eventos via @Output() para notificar o componente pai sobre interações do usuário.
Não devem ter dependências de serviços (exceto talvez serviços de utilidade, como um ToastService para feedback visual).
Não devem fazer chamadas HTTP diretas.
São altamente reutilizáveis.
Componentes Contêiner (Smart/Container Components):
Focam na lógica de negócio e no gerenciamento de estado.
Buscam dados de serviços.
Passam dados para componentes de apresentação via @Input().
Respondem a eventos de componentes de apresentação via @Output().
Contêm a lógica para chamadas HTTP e manipulação de dados.
Geralmente estão associados a uma rota (como os componentes em src/app/modules/produtos/pages).
Comunicação entre Componentes:
Pai para Filho: Use @Input() para passar dados.
Filho para Pai: Use @Output() e EventEmitter para emitir eventos.
Comunicação entre Componentes Irmãos ou Desacoplados: Use um serviço compartilhado com BehaviorSubjects para um padrão Publisher/Subscriber.

3.2. Gerenciamento de Estado com BehaviorSubjects

Para gerenciamento de estado local ou global dentro de um módulo (ou mesmo em toda a aplicação para estados menores), utilize BehaviorSubjects em serviços:
Serviços de Estado: Crie um serviço dedicado para gerenciar o estado de uma entidade ou recurso específico.
Propriedades Privadas: O BehaviorSubject deve ser uma propriedade private no serviço.
Observable Público: Exponha um Observable público (usando .asObservable()) para que os componentes possam se inscrever e reagir às mudanças de estado.
Métodos para Atualizar o Estado: Crie métodos públicos no serviço para atualizar o valor do BehaviorSubject.
Exemplo (conceitual):
TypeScript
// produto.service.ts (exemplo ilustrativo para gerenciamento de estado)
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Produto } from '../models/produto'; //

@Injectable({
  providedIn: 'root'
})
export class ProdutoStateService {
  private _produtos = new BehaviorSubject<Produto[]>([]);
  public readonly produtos$: Observable<Produto[]> = this._produtos.asObservable();

  constructor(private produtoService: ProdutoService) { } //

  carregarProdutos(): void {
    this.produtoService.listar().subscribe( //
      (produtos) => this._produtos.next(produtos),
      (error) => console.error('Erro ao carregar produtos:', error)
    );
  }

  adicionarProduto(produto: Produto): void {
    this.produtoService.criar(produto).subscribe( //
      (novoProduto) => {
        const currentProdutos = this._produtos.getValue();
        this._produtos.next([...currentProdutos, novoProduto]);
      },
      (error) => console.error('Erro ao adicionar produto:', error)
    );
  }
  // ... outros métodos para atualizar, deletar, etc.
}



3.3. Injeção de Dependências

Sempre use o sistema de injeção de dependências do Angular.
Injete serviços nos construtores dos componentes e outros serviços.
Prefira injeções no root (usando providedIn: 'root' no @Injectable) para serviços que precisam ser singletons e compartilhados por toda a aplicação, ou que não são específicos de um módulo.
Para serviços específicos de um módulo, injete-os no providers do @NgModule desse módulo.

3.4. Formulários Reativos

Sempre prefira Formulários Reativos sobre Formulários de Template para formulários complexos, validações dinâmicas ou quando o estado do formulário precisa ser programaticamente controlado. O cadastro-produto.component.ts já utiliza essa abordagem, o que é excelente.
Estrutura do FormGroup: Defina seu FormGroup e FormControls no método ngOnInit ou em um método dedicado (criarFormulario()) no componente.
Validação: Utilize os validadores embutidos do Angular (Validators.required, Validators.minLength, Validators.min, etc.). Para validações personalizadas, crie funções validadoras.
Tratamento de Erros de Formulário:
Acesse o estado de validação de cada FormControl no template para exibir mensagens de erro ao usuário (ex: produtoForm.get('descricao')?.hasError('required') && produtoForm.get('descricao')?.touched).
Marque os controles como touched ou dirty no onSubmit() para exibir erros apenas após a interação do usuário ou tentativa de submissão.

3.5. Uso de Módulos

SharedModule: O SharedModule é uma excelente prática para agrupar módulos comuns do Angular (CommonModule, FormsModule, ReactiveFormsModule, RouterModule) e componentes, pipes e diretivas que serão usados por múltiplos módulos da aplicação.
Não deve ter serviços no providers: Componentes, pipes e diretivas do SharedModule devem ser declarations e exports, mas o módulo Shared em si não deve ter providers, para evitar que sejam criadas múltiplas instâncias de serviços.
Importe o SharedModule em outros módulos que precisam de suas funcionalidades, como o ProdutosModule.
Módulos de Funcionalidade (Lazy Loading): Mantenha a prática de módulos de funcionalidade (ex: ProdutosModule, EstoqueModule) com lazy loading. Isso melhora o tempo de carregamento inicial da aplicação, carregando apenas o código necessário para a rota atual.
CoreModule (Opcional): Para serviços singletons ou componentes que são usados apenas uma vez na aplicação (como o layout principal), considere criar um CoreModule que seja importado apenas uma vez, no AppModule (ou no main.ts para aplicações standalone como a sua). Atualmente, seu ToastService é providedIn: 'root', o que já o torna um singleton global.

Fase 3: Manipulação de Dados e Comunicação com a API

Concluído: Fases 1 e 2.
Próximos Passos: Detalhar padrões para serviços, tratamento de dados, HttpClient e RxJS.
Fases Restantes: 4, 5, 6.

4. Manipulação de Dados e Comunicação com a API


4.1. Padrões para Serviços que Interagem com a API (CRUD)

Responsabilidade Única: Cada serviço deve ser responsável por uma única entidade ou recurso da API (ex: ProdutoService para operações com Produto).
Métodos CRUD Padrão: Implemente métodos para operações básicas (CRUD): listar, buscarPorId, criar, atualizar, excluir.
Métodos de Consulta Específicos: Para consultas mais complexas, crie métodos nomeados de forma clara (ex: buscarPorGrupo, buscarPorMarca, buscarPorCodigoEstoque).
Tratamento de Parâmetros: Utilize HttpParams para construir parâmetros de query de forma segura e legível.

4.2. Tratamento de Dados Recebidos da API

Interfaces/Modelos: Defina interfaces ou classes para tipar os dados que vêm da API, garantindo segurança de tipo e clareza no código (ex: Produto, ProdutoFiltro).
Mapeamento de Dados: Se a estrutura dos dados da API for diferente da estrutura do modelo no frontend, use operadores RxJS (map) para transformar os dados para o formato esperado.
Datas: Garanta que as datas sejam corretamente convertidas entre strings (do backend) e objetos Date (no frontend) e vice-versa. O ProdutoService já faz um bom trabalho formatando datas para o backend. No lado da leitura, o Angular HttpClient pode, às vezes, parsear strings ISO 8601 para objetos Date automaticamente, mas é sempre bom verificar e converter explicitamente se necessário (como feito no cadastro-produto.component.ts ao preencher o formulário).

4.3. Regras para Uso do HttpClient e Tratamento de Erros HTTP

Injeção: Injete HttpClient no construtor dos seus serviços de API.
Tratamento de Erros (Frontend):
Capture erros de requisições HTTP usando o operador catchError do RxJS, que permite interceptar e tratar erros de forma reativa.
Utilize o ToastService para exibir mensagens de erro amigáveis ao usuário em caso de falha na requisição, como já implementado no cadastro-produto.component.ts.
No subscribe, sempre inclua um bloco error para lidar com falhas específicas da requisição.
Interceptors (Próximo Passo/Opcional): Para um tratamento de erros e autenticação mais global e centralizado, considere implementar HttpInterceptors futuramente. Isso permite adicionar tokens de autenticação a todas as requisições ou lidar com erros de forma padronizada em um único local, sem replicar a lógica em cada serviço. (Isso será abordado mais em detalhes na Fase 5).

4.4. Recomendações para o Uso de Observables e Operadores RxJS

Imutabilidade: Ao manipular dados de Observables (especialmente BehaviorSubjects), sempre trabalhe com cópias dos dados (imutabilidade) para evitar efeitos colaterais indesejados e garantir que o Change Detection do Angular funcione corretamente.
Operadores Comuns:
map: Para transformar os dados emitidos por um Observable.
filter: Para filtrar dados com base em uma condição.
tap: Para executar efeitos colaterais (como logs ou chamadas a um ToastService) sem modificar o stream.
switchMap, mergeMap, concatMap, exhaustMap: Para lidar com Observables aninhados (muito comuns em chamadas de API encadeadas). switchMap é frequentemente a escolha padrão para autocompletar ou quando apenas a última requisição é importante.
takeUntil, takeWhile, first: Para cancelar subscriptions e evitar vazamentos de memória (veja Fase 4).
Unsubscribe: É crucial desinscrever-se de Observables que não são gerenciados pelo async pipe para evitar vazamentos de memória. Isso será detalhado na Fase 4.

Fase 4: Performance e Otimização

Concluído: Fases 1, 2 e 3.
Próximos Passos: Detalhar otimizações de performance, lazy loading, Change Detection e prevenção de vazamentos de memória.
Fases Restantes: 5, 6.

5. Performance e Otimização


5.1. Lazy Loading de Módulos e Rotas

Uso Ativo: Continue utilizando o lazy loading para módulos de funcionalidade (ex: produtos, estoque) conforme configurado em app.routes.ts. Isso reduz o tamanho do bundle inicial, melhorando o tempo de carregamento da aplicação.
Granularidade: Avalie se sub-funcionalidades dentro de um módulo também poderiam se beneficiar de lazy loading, criando submódulos se a complexidade justificar.

5.2. Otimização do Change Detection

Estratégia OnPush: Considere usar a estratégia de detecção de mudanças OnPush para componentes de apresentação (dumb components). Com OnPush, o Angular só verifica o componente e seus filhos quando:
As entradas (@Input()) do componente mudam (comparação de referência).
Um evento é disparado pelo componente ou um de seus filhos.
Um Observable usado no template com o async pipe emite um novo valor.
A detecção de mudanças é explicitamente disparada (ChangeDetectorRef.detectChanges()).
Imutabilidade: Para que OnPush funcione de forma eficaz, é fundamental que os dados de entrada (@Input()) sejam imutáveis. Em vez de modificar arrays ou objetos existentes, crie novas referências ao atualizar seus valores.

5.3. Prevenção de Vazamentos de Memória (Unsubscribe)

async pipe: Sempre que possível, utilize o async pipe no template. Ele gerencia automaticamente as inscrições e desinscrições de Observables, evitando vazamentos de memória.
takeUntil Operator: Para subscriptions programáticas (em .ts), use o operador takeUntil do RxJS em conjunto com um Subject de controle. Esta é a abordagem recomendada para gerenciar o ciclo de vida de subscriptions.
TypeScript
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ProdutoService } from '../../services/produto.service'; //

@Component({ /* ... */ })
export class MeuComponente implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  constructor(private produtoService: ProdutoService) { } //

  ngOnInit(): void {
    this.produtoService.listar() //
      .pipe(takeUntil(this.destroy$))
      .subscribe(produtos => {
        // ... lidar com os produtos
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}


Outras Opções (Menos Recomendadas):
take(1): Útil para Observables que emitem apenas um valor e completam.
first(): Similar ao take(1), mas emite um erro se o Observable completar sem emitir nenhum valor.

5.4. Considerações sobre o Tamanho dos Bundles

Análise de Bundles: Use ferramentas como webpack-bundle-analyzer (integrado ao @angular/cli) para visualizar o conteúdo dos seus bundles e identificar grandes dependências que podem ser otimizadas ou carregadas sob demanda.
Tree Shaking: O Angular CLI e o Webpack já realizam tree shaking automaticamente, removendo código não utilizado. Escreva código modular para maximizar o efeito do tree shaking.
Otimização de Assets: Otimize imagens e outros assets.

Fase 5: Segurança e Gerenciamento de Dependências

Concluído: Fases 1, 2, 3 e 4.
Próximos Passos: Detalhar segurança, gerenciamento de dependências e sugestões para autenticação/autorização.
Fases Restantes: 6.

6. Segurança e Gerenciamento de Dependências


6.1. Diretrizes Iniciais para Segurança no Frontend

Prevenção de XSS (Cross-Site Scripting):
O Angular sanitiza automaticamente valores ao usar interpolação {{ }} e property binding [prop]="value", o que ajuda a prevenir XSS.
Quando for necessário injetar HTML diretamente (ex: usando [innerHTML]), sempre sanitize o HTML usando o DomSanitizer do Angular para garantir que apenas conteúdo seguro seja renderizado.
Evitar Injeção de Código Malicioso:
Nunca use eval() ou construa JavaScript dinamicamente a partir de dados fornecidos pelo usuário.
Tenha cuidado ao usar APIs DOM diretamente que possam introduzir vulnerabilidades.
Proteção de Dados Sensíveis:
Não armazene informações sensíveis (senhas, tokens de API críticos) no armazenamento local (localStorage, sessionStorage) ou em cookies não seguros, pois são vulneráveis a ataques XSS. Tokens de autenticação devem ter tempo de vida limitado e serem gerenciados com cuidado.

6.2. Gerenciamento de Dependências NPM

Auditoria de Vulnerabilidades: Execute npm audit regularmente para verificar se há vulnerabilidades de segurança nas suas dependências e atualize ou aplique patches conforme necessário.
Atualização de Pacotes:
Versões Fixas (~ ou ^): O Angular CLI por padrão usa ^ no package.json, o que permite atualizações de minor versions automaticamente. Para equipes juniores, isso pode ser benéfico para manter-se atualizado com patches e pequenas melhorias.
Testes de Regressão: Após atualizar dependências, execute os testes existentes para garantir que não houve quebras. Para atualizações de major versions, consulte as guias de migração (ng update).
Remover Dependências Não Utilizadas: Periodicamente, revise o package.json e remova dependências que não são mais necessárias para reduzir o tamanho do bundle e a superfície de ataque.

6.3. Sugestões para Lidar com Autenticação e Autorização (Próximo Passo)

Quando a autenticação e autorização forem implementadas, considere os seguintes padrões:
Autenticação Baseada em Tokens (JWT):
Após o login, o servidor retorna um JSON Web Token (JWT).
Armazene o JWT de forma segura (ex: em localStorage ou sessionStorage, com as devidas precauções, ou em HttpOnly cookies para maior segurança contra XSS).
Envie o JWT em cada requisição para o backend (geralmente no cabeçalho Authorization: Bearer <token>).
Http Interceptors para Tokens:
Crie um HttpInterceptor para automaticamente anexar o JWT a todas as requisições HTTP enviadas ao backend. Isso centraliza a lógica de autenticação.
Guards de Rota (CanActivate):
Utilize AuthGuards (guardas de rota) para proteger rotas no frontend, impedindo que usuários não autenticados acessem certas páginas.
Guards também podem ser usados para autorização, verificando se o usuário tem permissão para acessar uma rota específica com base em suas roles (perfis).
Serviço de Autenticação:
Crie um serviço dedicado (AuthService) para lidar com a lógica de login, logout, registro, renovação de tokens e gerenciamento do estado de autenticação do usuário (ex: usando um BehaviorSubject para currentUser$ ou isAuthenticated$).

Fase 6: Manutenção e Qualidade de Código

Concluído: Fases 1, 2, 3, 4 e 5.
Próximos Passos: Detalhar diretrizes para código limpo, legibilidade, prevenção de duplicação e refatoração.
Fases Restantes: Nenhuma.

7. Manutenção e Qualidade de Código


7.1. Diretrizes para Escrita de Código Limpo e Legível

Nomenclatura Significativa: Conforme detalhado na Fase 1, utilize nomes claros e descritivos para variáveis, funções, classes, etc., que revelem sua intenção e propósito.
Comentários (Quando Necessário):
Evite comentários óbvios que apenas repetem o que o código já diz.
Use comentários para explicar o "porquê" (a razão de uma decisão, uma lógica complexa) em vez do "o quê".
Comentários de JSDoc para documentar funções, classes e seus parâmetros são altamente recomendados para facilitar o entendimento da API do código.
Formatação Consistente: Use uma ferramenta como Prettier e/ou as configurações de formatação do VS Code para garantir que todo o código siga um estilo consistente (indentação, quebra de linha, espaçamento).
Organização Lógica: Agrupe funcionalidades relacionadas. Por exemplo, no CadastroProdutoComponent, os métodos de carregamento de dados (carregarGrupos, carregarMarcas, etc.) são agrupados. Mantenha métodos públicos antes de métodos privados.
Evitar Magic Strings/Numbers: Utilize constantes ou enums para valores literais que se repetem ou têm significado especial (ex: ProdutoStatus.Ativo em vez de 1).

7.2. Evitar Duplicação de Código (DRY - Don't Repeat Yourself)

Componentes Reutilizáveis: Se uma parte da UI ou um trecho de HTML/CSS/TS se repete em vários locais, considere criar um componente de apresentação (dumb component) para encapsular essa lógica.
Serviços Compartilhados: Se a lógica de negócio ou a interação com a API for utilizada por múltiplos módulos, mova-a para um serviço compartilhado (colocado em src/app/services ou dentro de src/app/shared).
Pipes e Diretivas: Para transformações de dados ou manipulações do DOM que se repetem, crie pipes ou diretivas personalizadas e exporte-as via SharedModule.
Funções Utilitárias: Para funções puras (que não dependem do estado da aplicação ou de injeção de dependências) que são reutilizáveis, crie arquivos de utilitários (ex: utils.ts) em uma pasta utils dentro de src/app/shared.

7.3. Refatoração e Manutenção

Tamanho de Arquivos e Funções: Conforme mencionado na Fase 1, refatore arquivos que excedem 200-300 linhas e funções que excedem 20-30 linhas. Divida-os em partes menores e mais gerenciáveis.
Revisão de Código (Code Review): Implemente um processo de revisão de código. A revisão por pares ajuda a identificar bugs, melhorias de desempenho e violações de padrões antes que o código seja integrado.
Remoção de Código Morto: Exclua código comentado ou não utilizado. Mantenha seu codebase limpo e relevante.
Technical Debt: Esteja ciente da dívida técnica. Identifique áreas que precisam de refatoração, documente-as e planeje sua resolução em futuras iterações.
Este arquivo único contém todas as fases do plano, fornecendo um guia abrangente para as regras de codificação do seu projeto Angular.
