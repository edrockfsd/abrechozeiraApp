# Registro de Histórico: Migração, Motor de IA e Emissão Fiscal NFC-e SEFAZ-PR
**Data:** 18 a 19 de Agosto de 2026  
**Ambiente:** Desenvolvimento (`abrechozeira`) & Produção (`abrechozeira01`)

---

## 1. Contexto Geral e Objetivos da Sessão

Nesta sessão, foram realizadas operações críticas de infraestrutura de dados, evolução do motor de inteligência artificial para cadastro de produtos de vestuário e correção/autorização da emissão de NFC-e em lote com a SEFAZ-PR.

---

## 2. Bancos de Dados e Migração Segura

### 2.1 Identificação das Conexões
- **DEV**: `Server=mysql.abrechozeira.com.br;Port=3306;Database=abrechozeira;Uid=abrechozeira;`
- **PROD**: `Server=mysql.abrechozeira.com.br;Port=3306;Database=abrechozeira01;Uid=abrechozeira01;`
- Senha de PROD alinhada no `appsettings.json`.

### 2.2 Backup Integral de Produção
- Realizado backup completo da base de produção antes de qualquer manipulação de dados:
  - Arquivo: `BKP/20260818_pre_migracao/backup_abrechozeira01_20260818_171755.sql` (425 KB, 2.037 registros exportados, 43 tabelas).
  - Garantia de que a base de produção permaneceu **100% inalterada** (acesso estritamente read-only).

### 2.3 Migração de Cadastros Base para DEV
- Foram sincronizados de PROD para DEV os cadastros estruturais para permitir testes reais:
  - `Pessoa` (287 registros sincronizados)
  - `Endereco` (286 registros)
  - `User` (284 registros)
  - `tb_enviolote_map` (305 registros)
  - Tabelas de domínio (`Role`, `Permission`, `PessoaTipo`, `PessoaGenero`, `PessoaCategoria`, etc.).

---

## 3. Motor Inteligente de Categorização e IA (`ProdutoIAService.cs`)

### 3.1 Dicionário de Abreviações de Marcas
Implementado reconhecimento determinístico de alta precisão para as 17 marcas e suas abreviações fornecidas:
- `TH` ➔ Tommy Hilfiger (`Id: 1`)
- `GAP` ➔ GAP (`Id: 2`)
- `MK` ➔ Michael Kors (`Id: 3`)
- `LP` ➔ Lança Perfume (`Id: 4`)
- `GUESS` ➔ GUESS (`Id: 5`)
- `Adidas` ➔ Adidas (`Id: 6`)
- `Nike` ➔ Nike (`Id: 7`)
- `CS` ➔ Carmen Steffens (`Id: 8`)
- `Levis` / `Levi's` ➔ Levis (`Id: 9`)
- `Outros` ➔ Outros (`Id: 10`)
- `Felini` ➔ Felini (`Id: 11`)
- `Animale` ➔ Animale (`Id: 12`)
- `Columbia` ➔ Columbia (`Id: 13`)
- `TNF` ➔ The North Face (`Id: 14`)
- `LLB` ➔ Le Lis Blanc (`Id: 15`)
- `Zara` ➔ Zara (`Id: 16`)
- `Farm` ➔ Farm (`Id: 17`)

### 3.2 Regras de Negócio de Grupos de Vestuário
- Ajustado o motor para mapear peças de tipo **`Fleece`**, **`Ted`** e **`Teddy`** para o `GrupoId: 7` (**Blusas**).
- Mapeamento determinístico de grupos como **Camisetas**, **Blusas**, **Jaquetas**, **Casacos**, **Calças**, **Vestidos**, tamanhos e perfis.

### 3.3 Regra de Auto-Cadastro de Pessoas na Importação de Lives
- Na importação da planilha de arremates via `LiveImportController`, caso o `@nickname` do Instagram do comprador não exista na base:
  - O sistema realiza um **auto-cadastro mínimo ativo** em `Pessoa` com `Nome = NickName = @comprador`.
  - Gera o `Pedido` e a `Venda` sem bloquear o processamento e sem perder os dados arrematados.

---

## 4. Emissão Fiscal de NFC-e (SEFAZ-PR)

### 4.1 Diagnóstico e Correção da Rejeição 391 SEFAZ-PR
- **Problema Inicial:** A emissão de NFC-e com pagamento PIX/Cartão retornava `[391] Nao informados os dados do cartão de crédito / débito nas Formas de Pagamento da Nota Fiscal`, e por isso a nota não aparecia no portal SEFAZ ("registro eletrônico não encontrado").
- **Causa Fiscal:** A SEFAZ do Paraná (Regra YA01-20 / NT 2020.006) exige que para pagamentos eletrônicos (`tPag = 17` para PIX, `03` para Crédito, `04` para Débito), seja enviada a tag:
  ```xml
  <card>
      <tpIntegra>2</tpIntegra>
  </card>
  ```
- **Solução Implementada:** Atualizado `NfceService.cs` para incluir automaticamente `<card><tpIntegra>2</tpIntegra></card>` para pagamentos não em dinheiro.
- **Resultado do Teste Real:** Transmissão autorizada com sucesso nos servidores de Homologação da SEFAZ-PR:
  - **Status:** `Autorizada`
  - **Protocolo:** `141260001535978`
  - **Mensagem:** `[104] Lote processado`
  - **Chave de Acesso:** `41260848749443000106650010000000141100000002` (NFC-e Nº 14)

### 4.2 Ajustes no Grid de Emissão em Lote (Frontend)
- Adicionada a propriedade `isPrimaryKey="true"` na coluna `id` do componente Syncfusion (`ejs-grid`) em `sales-list.component.html`, necessária para o correto funcionamento do `persistSelection: true`.
- Implementado fallback no método `emitirNfceLote()` em `sales-list.component.ts` com `getSelectedRowIndexes()`.

### 4.3 Visualização e Cópia de Chaves de Acesso
- O endpoint `GET /api/Vendas/listagem` e a interface `VendaListItem` agora retornam `nfceChave`, `nfceNumero` e `nfceProtocolo`.
- No frontend:
  - Exibição do chip com final da chave e tooltip com chave completa + protocolo.
  - Botão de **cópia instantânea da chave** com feedback visual.
  - Botão **"Ver Cupom"** direcionando para `SalesReceiptComponent` com DANFE, chave formatada, protocolo e QR Code oficial SEFAZ.

---

## 5. Arquivos Modificados no Versionamento

- `API/Controllers/VendasController.cs`
- `API/Services/NfceService.cs`
- `API/Services/ProdutoIAService.cs`
- `FRONT/src/app/modules/pdv/pages/sales-list/sales-list.component.html`
- `FRONT/src/app/modules/pdv/pages/sales-list/sales-list.component.scss`
- `FRONT/src/app/modules/pdv/pages/sales-list/sales-list.component.ts`
- `FRONT/src/app/services/vendas.service.ts`
- `BKP/20260818_pre_migracao/backup_abrechozeira01_20260818_171755.sql`
- `docs/HISTORICO_MIGRACAO_E_AJUSTES_FISCAIS_20260818.md`
