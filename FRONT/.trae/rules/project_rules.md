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
