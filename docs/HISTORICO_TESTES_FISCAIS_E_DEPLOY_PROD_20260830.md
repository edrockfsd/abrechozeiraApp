# Histórico de Ajustes Fiscais, Correções e Entrada em Produção (30/08/2026)

## 1. Resumo da Sessão

Nesta sessão, foram concluídos os testes de geração de NFC-e a partir de planilhas de Live, implementada a identificação completa do consumidor no documento fiscal (tanto para controle interno quanto perante a SEFAZ-PR), corrigida uma inconsistência crítica na categorização de novos clientes cadastrados via Live e configurado o ambiente oficial de **Produção da SEFAZ-PR** no banco de dados de produção (`abrechozeira01`).

---

## 2. Endpoint de Limpeza e Reset de Dados de Live

- **Arquivo:** `API/Controllers/LiveImportController.cs`
- **Endpoint:** `DELETE /api/LiveImport/limpar-live/{liveId}` (`[AllowAnonymous]`)
- **Objetivo:** Permitir ao usuário resetar os dados de testes de uma Live antes de reimportar a planilha.
- **Correção de Chave Estrangeira (FK):**
  - Inicialmente, a remoção falhava com violação de FK (`FK_Nfce_Pedido_PedidoId`), pois a query buscava NFC-e apenas por `VendaId`.
  - Foi ajustado para coletar todos os `pedidoIds` (inclusive criados por observações) e buscar notas por `(n.VendaId.HasValue && vendaIds.Contains(n.VendaId.Value)) || (n.PedidoId.HasValue && pedidoIds.Contains(n.PedidoId.Value))`.
  - A exclusão em cascata agora remove rigorosamente na ordem correta:
    1. `NfcePagamento`
    2. `NfceItem`
    3. `Nfce`
    4. `Venda`
    5. `PedidoProduto`
    6. `Pedido`
    7. `Arremate`
    8. `Estoque` e `Produto` gerados na Live.

---

## 3. Identificação do Comprador na NFC-e e DANFE

### 3.1 Informações Complementares (`<infAdic><infCpl>`)
- **Arquivo:** `API/Services/NfceService.cs`
- **Regra SEFAZ:** Posicionado estritamente entre `</pag>` e `<infRespTec>`.
- **Formato:**
  ```xml
  <infAdic>
      <infCpl>COMPRADOR: {Nome} (@{NickName}) | PEDIDO: #{PedidoId} | VENDA: #{VendaId}</infCpl>
  </infAdic>
  ```
- Garante que a nota fiscal tenha rastreabilidade e referência ao comprador do Instagram mesmo se o consumidor não informar CPF.

### 3.2 Identificação Oficial na SEFAZ (`<dest>`)
- **Arquivo:** `API/Services/NfceService.cs`
- **Regra SEFAZ-PR:** A tag `<dest>` em NFC-e (modelo 65) só pode ser enviada se contiver documento válido (`<CPF>` ou `<CNPJ>`). Não se pode enviar nome desacompanhado de documento, nem CPF inválido (causa Rejeição 237).
- **Implementação:**
  - Adicionados validadores matemáticos de dígitos verificadores (`ValidarCpf` e `ValidarCnpj`).
  - Se o cliente possuir CPF válido cadastrado, gera a tag `<dest>` completa após `</emit>`:
    ```xml
    <dest>
        <CPF>{cpfLimpo}</CPF>
        <xNome>{nomeCliente}</xNome>
        <indIEDest>9</indIEDest>
    </dest>
    ```
  - Se o documento não for válido ou estiver ausente, a tag `<dest>` é omitida, evitando rejeição da SEFAZ.

### 3.3 Visualização no Cupom DANFE (Frontend)
- **Arquivos:**
  - `FRONT/src/app/modules/pdv/pages/sales-receipt/sales-receipt.component.html`
  - `FRONT/src/app/modules/pdv/pages/sales-receipt/sales-receipt.component.scss`
- Adicionado bloco visual de **DADOS DO CONSUMIDOR** contendo Nome, Instagram e Pedido, além de exibir as informações complementares de interesse do contribuinte no rodapé.

---

## 4. Correção na Categorização de Novos Clientes (Live)

### 4.1 Problema Identificado
- Clientes novos cadastrados através da planilha de live (ex: `daeale2021`) apareciam na listagem com a Categoria **"Administrador"** em vez de **"Cliente"**.

### 4.2 Causa Raiz
- Na tabela `PessoaCategoria` do MySQL:
  - `Id = 1`: **Administrador**
  - `Id = 2`: **Cliente**
- No `LiveImportController.cs`, ao realizar o auto-cadastro mínimo de pessoas não cadastradas, o código estava fixando `PessoaCategoriaId = 1`.

### 4.3 Solução Implementada
1. **Código:** Atualizado `LiveImportController.cs` para buscar dinamicamente no banco a categoria `"Cliente"` (`Id: 2`), tipo `"Física"` (`Id: 1`) e gênero `"Feminino"` (`Id: 1`).
2. **Correção de Dados:** 
   - A cliente `daeale2021` (`Id: 332`) foi alterada para `PessoaCategoriaId = 2`.
   - Outros 16 compradores de lives anteriores que haviam herdado `Id: 1` foram corrigidos no banco para `PessoaCategoriaId = 2`. Restaram apenas os administradores reais (`Admin` e `Sarah`).

---

## 5. Entrada em Produção e Parâmetros Fiscais (SEFAZ-PR)

### 5.1 Parâmetros Homologados
- **Empresa:** A BRECHOZEIRA LTDA
- **CNPJ:** `48.749.443/0001-06` | **IE:** `9123965768`
- **Ambiente SEFAZ:** `1` (**Produção**)
- **CSC Token (Produção):** `ZFEHCXTSFAIF7HPMEKFYWPSWTEFMOSVIFDXY`
- **CSC Id (cIdToken):** `000001`
- **Série da NFC-e:** `1`
- **Próximo Número:** `1` (primeira emissão em produção pelo CNPJ)
- **Certificado Digital A1:** `Certificados/cert_20260811195501.pfx`

### 5.2 Ajustes de Deploy e Publicação
- **`ABrechozeiraApp.csproj`:** Incluída a pasta `Certificados\**` no `CopyToPublishDirectory` com `PreserveNewest`, garantindo que os certificados sejam enviados no artefato de build do GitHub Actions para a KingHost.
- **`NfceService.cs`:** Atualizado para aceitar caminhos relativos de certificado (resolvendo automaticamente via `AppContext.BaseDirectory`).
- **Banco de Produção (`abrechozeira01`):** A tabela `EmpresaFiscal` de produção foi criada e populada com os dados acima.

---

## 6. Próximos Passos
- Executar `git push origin main` para disparar o pipeline CI/CD no GitHub Actions.
- Acompanhar os estágios:
  1. `build-backend`
  2. `build-frontend`
  3. `migrate-database`
  4. `deploy` (KingHost via FTP com `app_offline.htm`).
