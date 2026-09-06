# Registro de Conversa, Arquitetura e Implementação da Gestão de Live (06/09/2026)

## 1. Visão Geral e Contexto da Sessão

Nesta sessão, foi arquitetado, implementado, testado em live real e validado o subsistema completo de **Gestão e Rastreio de Live ao Vivo (Live Tracker)** no ERP A.Brechozeira.

Anteriormente, a operação do brechó dependia do software externo "Social Stream Ninja" para monitorar o chat e preencher planilhas Google Sheets manualmente ou por scripts externos. O objetivo desta entrega foi internalizar 100% desse fluxo diretamente no sistema, conectando a leitura de chat oficial do Instagram à ordenação instantânea de arrematantes, auditoria de milissegundos para clientes e sincronização com Google Sheets em tempo real.

---

## 2. Solicitações do Usuário e Evolução dos Requisitos

1. **Internalização do Gestor de Live:**
   - Possuir tela dedicada com os campos: *Código da Peça*, *Descrição da Peça* e *Valor (R$)*.
   - Ao iniciar o rastreio do código, varrer retroativamente quem já digitou a peça nos últimos minutos e continuar monitorando em tempo real.
   - Definir o **1º lugar como Comprador oficial** e os demais como **Fila de Espera** (2º, 3º, 4º, etc.).
   - Gravar o arremate na tabela `Arremate` e opcionalmente sincronizar no Google Sheets da live.

2. **Precisão de Milissegundos e Auditoria de Chat:**
   - Clientes frequentemente contestam a ordem de quem digitou primeiro na live.
   - Exigência: ordenação estrita por tempo e modal de auditoria com botão para copiar a prova do chat completa (com carimbo de hora até os milissegundos) para enviar no WhatsApp das clientes.

3. **Refinamento do Matching de Código (Strict Match):**
   - O código digitado deve ser **estritamente idêntico** ao cadastrado pelo operador:
     - `"055"` NÃO bate com `"55"`.
     - `"001"` NÃO bate com `"1"` ou `"01"`.
     - `"flor"` NÃO bate com `"fl0r"`.
   - Regra: `string.Equals(t, c, StringComparison.OrdinalIgnoreCase)` puro com trim nas extremidades.

4. **Uso de `CreatedAt` ao invés de `CommentTimestamp`:**
   - A API do Instagram muitas vezes agrupa os comentários no mesmo segundo ou com atraso no `timestamp`.
   - O campo `CreatedAt` do banco local grava o `DateTime.Now` de recebimento com precisão sub-milissegundo, sendo a fonte da verdade oficial para ordenação de fila e auditoria.

5. **Formatação da Planilha Google Sheets:**
   - A fila nas colunas `Fila 1`, `Fila 2`, `Fila 3`, etc. deve conter **apenas os nomes de usuários** (@usuario), sem carimbo de hora ou texto digitado.

6. **Detecção Robusta de Lives Ativas:**
   - O Instagram retorna `media_product_type: "FEED"` mesmo durante lives ativas. Ajustou-se para selecionar a transmissão criada nas últimas 4 horas cujo `media_type == "BROADCAST"`.
   - Limpeza automática de sessões antigas no banco ao iniciar o serviço.

7. **Múltiplas Lives no Mesmo Dia:**
   - Quando mais de uma live for aberta na mesma data, nomear sequencialmente de forma automática:
     - `Live 06/09/2026 01`
     - `Live 06/09/2026 02`
     - `Live 06/09/2026 03`...
   - Vincular os comentários diretamente à transmissão específica via `LiveVideoId`.

8. **Eliminação de Mensagens Duplicadas no Chat:**
   - Removido disparo redundante no SignalR (que notificava tanto a sala da live quanto a sala do vídeo).
   - Adicionada deduplicação por ID e conteúdo no frontend Angular.

---

## 3. Arquitetura Técnica Implementada

### 3.1 Backend (.NET 8 Web API + SignalR)

* **`API/Hubs/LiveHub.cs`:**
  - Hub SignalR para streaming de dados em tempo real.
  - Grupos de salas dinâmicos: `Live_{liveId}`.
  - Eventos disparados: `NovoComentario`, `StatusRastreio`, `MatchDetectado`, `MatchesAtualizados`.

* **`API/Services/LiveTrackerService.cs`:**
  - Gerencia o estado de rastreio em memória com `ConcurrentDictionary<int, LiveTrackingState>` e bloqueio concorrente `lock`.
  - Buffer circular das últimas 500 mensagens de chat.
  - Varredura retroativa de até 5 minutos no banco de dados com ordenação `OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)`.
  - Match estrito por igualdade de texto.
  - Auditoria completa de chat para envio aos clientes.

* **`API/Services/InstagramLivePollingService.cs`:**
  - Polling resiliente de 1 segundo utilizando o Access Token oficial da Meta (`IGAA...`).
  - Consulta `GET /me/live_media?fields=id,timestamp,media_type,media_product_type`.
  - Captura comentários em `GET /{liveMediaId}/comments?fields=id,text,from,timestamp`.
  - Proteção contra rate limit lendo cabeçalhos `X-App-Usage`.
  - Limpeza automática de sessões anteriores pendentes.
  - Nomeação sequencial automática (`Live dd/MM/yyyy 01`, `02`, `03`...).

* **`API/Services/GoogleSheetsSyncService.cs`:**
  - Conexão autenticada via Google Service Account (`Google.Apis.Sheets.v4`).
  - Criação automática do cabeçalho caso a planilha esteja vazia.
  - Inserção de linhas em tempo real com `INSERTROWS`:
    - `[Código, Intern, Descrição, Valor, Comprador, Fila 1, Fila 2, Fila 3, ...]`
    - Fila preenchida estritamente com os usernames.

* **`API/Controllers/LiveTrackerController.cs`:**
  - `POST api/LiveTracker/iniciar-rastreio`
  - `POST api/LiveTracker/parar-rastreio/{liveId}`
  - `DELETE api/LiveTracker/remover-match/{liveId}/{index}`
  - `GET api/LiveTracker/estado-atual/{liveId}`
  - `POST api/LiveTracker/confirmar-arremate`
  - `GET api/LiveTracker/auditoria-codigo/{liveId}`

---

### 3.2 Frontend (Angular 19 + Syncfusion + SignalR Client)

* **`FRONT/src/app/modules/live-sessions/pages/gestao-live/gestao-live.component.ts|html|scss`:**
  - Interface Dark Mode responsiva e intuitiva inspirada em centros de comando.
  - Painel de controle da peça com atalhos de teclado (Enter para iniciar, Space para parar).
  - Feed de chat ao vivo com auto-scroll e destaque animado de "ACERTOU!" quando o comentário acerta o código.
  - Card dourado do Comprador oficial com carimbo de tempo milissegundo.
  - Lista de fila de espera com badges ordenados e botão para remoção/pulo individual.
  - Modal de Auditoria de Prova do Chat com geração de texto formatado e botão "Copiar Prova para WhatsApp".
  - Histórico de peças arrematadas na sessão atual.

* **`FRONT/src/app/modules/lives/pages/lista-lives/lista-lives.component.ts|html`:**
  - Card de alerta vermelho de transmissão ativa no topo com botão "Operar Live Agora" e botão para dispensar.
  - Filtro para exibir apenas lives ativas nas últimas 4 horas.
  - Navegação direta para a Live correspondente ao `liveVideoId`.

---

## 4. Registro Resumido do Diálogo e Ações Realizadas

| Momento | Usuário | Assistente / Ação |
|---|---|---|
| Início | "deploy ok, podemos testar agora? [...] enviamos vários comentários" | Validação do polling e gravação no banco de dados. |
| Arquitetura | "certo, agora precisamos passar para a parte do gerenciamento da live. Verifique a pasta brecho-live-tracker..." | Análise do código legado do Social Stream Ninja e arquitetura completa da nova solução integrada no ERP. |
| Requisitos | Regras de ordenação, timestamp e backup de planilha. | Criação do plano técnico e aprovação. |
| Implementação | "pode colocar o milissegundo no registro tb" | Implementação de `LiveTrackerService`, `LiveHub`, `GoogleSheetsSyncService` e tela Angular `GestaoLiveComponent`. |
| Teste Inicial | "estou testando agora, com uma live no ar, mas ela não aparece na listagem de lives" | Correção da URL da API do Instagram para suporte ao token oficial `IGAA...` e ordenação das lives. |
| Ajustes de Regras | "1. Código idêntico (055 não bate com 55). 2. Usar CreatedAt. 3. Na fila da planilha apenas nomes sem hora." | Refatoração de `LiveTrackerService.IsMatch`, migração das consultas de ordenação para `CreatedAt` e ajuste do layout de colunas no Google Sheets. |
| Live Real 01 | "Tem algo errado já, não estou em live e o card de live está ativo" | Descoberta de sessões antigas com `EndedAt == null` e ajuste do descarte de transmissões antigas. |
| Live Real 02 | "estou com uma live em curso e novamente o sistema não detectou" | Identificado que a Meta retorna `media_product_type: FEED` mesmo ao vivo. Ajustada a verificação para considerar transmissões criadas nas últimas 4 horas. Live detectada e comentários capturados com sucesso em tempo real. |
| Múltiplas Lives | "Acho q por estarmos fazendo mais de uma live no dia, o sistema está se perdendo... Kd live deve ter seu nome diferenciado (01, 02...)" | Implementada nomeação sequencial automática (`Live dd/MM/yyyy 01`, `02`, etc.) e deduplicação no SignalR e frontend para eliminar balões repetidos. |
| Fechamento Deploy | "a princípio tudo certo, vamos fazer o deploy? Não esqueça de subir toda nossa conversa junto com o versionamento." | Documentação da sessão em `docs/`, consolidação dos commits e disparo da pipeline de deploy. |
| Pós-Deploy Produção | "fui fazer o teste em produção agora e uma live está como aberta, mas na verdade é do dia 31/08/2026." | **Hotfix Live Órfã / Falso Positivo**: Criação do endpoint `GET /api/LiveTracker/live-ativa` que consulta o estado em tempo real do polling em memória. Remoção do fallback perigoso `|| this.lives[0]` que abria lives antigas caso o `liveId` não estivesse preenchido. Auto-encerramento imediato de sessões no banco de dados (`Status = 'ended'`, `EndedAt = DateTime.Now`) quando o Instagram confirma que não há transmissão ativa. |
| Sync Google Sheets | "agora está tudo certo, mas o sistema não está enviando os dados para a planilha google." | **Hotfix Google Sheets em Produção**: 1) Credencial da service account embutida diretamente no `GoogleSheetsSyncService` como fallback resiliente (evitando falhas por arquivos ignorados no `.gitignore` e não presentes na KingHost). 2) Herança automática da `GoogleSheetUrl` em novas lives detectadas pelo polling. 3) Fallback automático para a planilha padrão na confirmação de arremates se a live não possuir URL configurada. 4) Novo endpoint `POST /api/LiveTracker/sincronizar-planilha/{liveId}` e botão na tela **"Enviar para Google Sheets"** para enviar em lote peças já arrematadas anteriormente. 5) Botão **"Salvar na Live"** nas configurações para vincular a planilha diretamente. |

---

## 5. Arquivos Versionados

* **Novos Arquivos:**
  - `API/Controllers/LiveTrackerController.cs`
  - `API/DTOs/LiveTrackerDtos.cs`
  - `API/Hubs/LiveHub.cs`
  - `API/Services/GoogleSheetsSyncService.cs`
  - `API/Services/LiveTrackerService.cs`
  - `FRONT/src/app/modules/live-sessions/pages/gestao-live/gestao-live.component.ts`
  - `FRONT/src/app/modules/live-sessions/pages/gestao-live/gestao-live.component.html`
  - `FRONT/src/app/modules/live-sessions/pages/gestao-live/gestao-live.component.scss`
  - `FRONT/src/app/modules/live-sessions/services/live-tracker.service.ts`
  - `docs/CONVERSA_E_HISTORICO_GESTAO_LIVE_TRACKER_20260906.md`

* **Arquivos Atualizados:**
  - `API/ABrechozeiraApp.csproj`
  - `API/Program.cs`
  - `API/Controllers/LiveSessionsController.cs`
  - `API/Controllers/LivesController.cs`
  - `API/Controllers/LiveTrackerController.cs` (endpoints de live ativa, sincronização em lote e configuração de planilha)
  - `API/DTOs/LiveTrackerDtos.cs` (DTOs `LiveAtivaInfo`, `SincronizarPlanilhaRequest`, `ConfigurarPlanilhaRequest`)
  - `API/Services/GoogleSheetsSyncService.cs` (credencial embutida resiliente para cloud/KingHost e sync retroativo)
  - `API/Services/InstagramLivePollingService.cs` (herança de GoogleSheetUrl e encerramento de órfãs)
  - `API/Services/LiveTrackerService.cs` (gestão thread-safe de live ativa)
  - `FRONT/package.json` e `package-lock.json`
  - `FRONT/src/app/modules/live-sessions/live-sessions.routes.ts`
  - `FRONT/src/app/modules/live-sessions/pages/gestao-live/gestao-live.component.html` (botões "Enviar para Google Sheets" e "Salvar na Live")
  - `FRONT/src/app/modules/live-sessions/pages/gestao-live/gestao-live.component.scss` (estilos dos novos botões de ação e config)
  - `FRONT/src/app/modules/live-sessions/pages/gestao-live/gestao-live.component.ts` (integração dos fluxos de sincronização)
  - `FRONT/src/app/modules/live-sessions/services/live-tracker.service.ts` (métodos de API de sincronização e configuração)
  - `FRONT/src/app/modules/lives/lives.routes.ts`
  - `FRONT/src/app/modules/lives/pages/lista-lives/lista-lives.component.ts`
  - `FRONT/src/app/modules/lives/pages/lista-lives/lista-lives.component.html`
  - `FRONT/src/app/modules/lives/pages/lista-lives/lista-lives.component.scss`
  - `.gitignore`


