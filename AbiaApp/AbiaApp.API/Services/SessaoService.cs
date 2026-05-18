using AbiaApp.API.Models;
using ClosedXML.Excel;
using Dapper;
using MySql.Data.MySqlClient;

namespace AbiaApp.API.Services
{
    /// <summary>
    /// Gerencia sessões de cadastro em memória
    /// </summary>
    public class SessaoService
    {
        private readonly string _connectionString;
        
        // Armazena sessões ativas (em produção, considerar usar cache distribuído)
        private static readonly Dictionary<string, SessaoCadastro> _sessoes = new();
        private static readonly object _lock = new();

        public SessaoService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Inicia uma nova sessão buscando o maior CodigoEstoque do banco
        /// </summary>
        public SessaoCadastro IniciarSessao(string? origemFixa = null)
        {
            int maiorCodigo = 0;

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                maiorCodigo = conn.QueryFirstOrDefault<int?>(
                    "SELECT MAX(CodigoEstoque) FROM Estoque"
                ) ?? 0;
            }

            var sessao = new SessaoCadastro
            {
                ProximoCodigo = maiorCodigo + 1,
                OrigemFixa = origemFixa
            };

            lock (_lock)
            {
                _sessoes[sessao.Id] = sessao;
            }

            Console.WriteLine($"[SESSÃO] Nova sessão criada: {sessao.Id} | Próximo código: {sessao.ProximoCodigo}");
            return sessao;
        }

        /// <summary>
        /// Obtém uma sessão existente
        /// </summary>
        public SessaoCadastro? ObterSessao(string sessaoId)
        {
            lock (_lock)
            {
                return _sessoes.TryGetValue(sessaoId, out var sessao) ? sessao : null;
            }
        }

        /// <summary>
        /// Adiciona um item à sessão e retorna o código atribuído
        /// </summary>
        public ItemSessao AdicionarItem(string sessaoId, ProdutoIA_IDs dados)
        {
            lock (_lock)
            {
                if (!_sessoes.TryGetValue(sessaoId, out var sessao))
                    throw new InvalidOperationException($"Sessão '{sessaoId}' não encontrada.");

                var item = new ItemSessao
                {
                    CodigoEstoque = sessao.ProximoCodigo,
                    Descricao = dados.Descricao,
                    Tamanho = dados.Tamanho,
                    PrecoVenda = dados.PrecoVenda,
                    PrecoCusto = dados.PrecoCusto,
                    Origem = !string.IsNullOrEmpty(sessao.OrigemFixa) ? sessao.OrigemFixa : dados.Origem,
                    MarcaId = dados.MarcaId > 0 ? dados.MarcaId : 10,  // Marca genérica
                    GrupoId = dados.GrupoId > 0 ? dados.GrupoId : 18,  // Grupo genérico
                    GeneroId = dados.GeneroId > 0 ? dados.GeneroId : 1,
                    PerfilId = dados.PerfilId > 0 ? dados.PerfilId : 1,
                    Condicao = dados.Condicao ?? "N"
                };

                sessao.Itens.Add(item);
                sessao.ProximoCodigo++;

                Console.WriteLine($"[SESSÃO {sessao.Id}] Item #{sessao.Itens.Count} adicionado | Código: {item.CodigoEstoque} | {item.Descricao}");
                return item;
            }
        }

        /// <summary>
        /// Remove o último item da sessão
        /// </summary>
        public ItemSessao? RemoverUltimoItem(string sessaoId)
        {
            lock (_lock)
            {
                if (!_sessoes.TryGetValue(sessaoId, out var sessao) || sessao.Itens.Count == 0)
                    return null;

                var ultimo = sessao.Itens[^1];
                sessao.Itens.RemoveAt(sessao.Itens.Count - 1);
                sessao.ProximoCodigo--;

                Console.WriteLine($"[SESSÃO {sessao.Id}] Último item removido | Código: {ultimo.CodigoEstoque}");
                return ultimo;
            }
        }

        /// <summary>
        /// Gera o Excel com todos os itens da sessão
        /// </summary>
        public byte[] GerarExcel(string sessaoId)
        {
            SessaoCadastro? sessao;
            lock (_lock)
            {
                if (!_sessoes.TryGetValue(sessaoId, out sessao))
                    throw new InvalidOperationException($"Sessão '{sessaoId}' não encontrada.");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Produtos");

            // Cabeçalhos
            var headers = new[] { 
                "CodigoEstoque", "Descricao", "Tamanho", "PrecoVenda", "PrecoCusto", 
                "Origem", "MarcaId", "GrupoId", "GeneroId", "PerfilId", "Condicao", "DataCadastro" 
            };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Dados
            int row = 2;
            foreach (var item in sessao.Itens)
            {
                worksheet.Cell(row, 1).Value = item.CodigoEstoque;
                worksheet.Cell(row, 2).Value = item.Descricao;
                worksheet.Cell(row, 3).Value = item.Tamanho;
                worksheet.Cell(row, 4).Value = item.PrecoVenda;
                worksheet.Cell(row, 5).Value = item.PrecoCusto;
                worksheet.Cell(row, 6).Value = item.Origem;
                worksheet.Cell(row, 7).Value = item.MarcaId;
                worksheet.Cell(row, 8).Value = item.GrupoId;
                worksheet.Cell(row, 9).Value = item.GeneroId;
                worksheet.Cell(row, 10).Value = item.PerfilId;
                worksheet.Cell(row, 11).Value = item.Condicao;
                worksheet.Cell(row, 12).Value = item.DataCadastro.ToString("yyyy-MM-dd HH:mm:ss");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            
            Console.WriteLine($"[SESSÃO {sessao.Id}] Excel gerado com {sessao.Itens.Count} itens");
            return stream.ToArray();
        }

        /// <summary>
        /// Finaliza e remove a sessão
        /// </summary>
        public void FinalizarSessao(string sessaoId)
        {
            lock (_lock)
            {
                if (_sessoes.Remove(sessaoId))
                {
                    Console.WriteLine($"[SESSÃO] Sessão {sessaoId} finalizada e removida");
                }
            }
        }

        /// <summary>
        /// Salva todos os itens da sessão no banco de dados em uma única transação (tudo ou nada)
        /// </summary>
        public int SalvarNoBanco(string sessaoId)
        {
            SessaoCadastro? sessao;
            lock (_lock)
            {
                if (!_sessoes.TryGetValue(sessaoId, out sessao))
                    throw new InvalidOperationException($"Sessão '{sessaoId}' não encontrada.");
                
                if (sessao.Itens.Count == 0)
                    throw new InvalidOperationException("Sessão não possui itens para salvar.");
            }

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                int statusId = 1; // Status padrão

                foreach (var item in sessao.Itens)
                {
                    // Insere o Produto
                    var sqlProduto = @"
                        INSERT INTO Produto 
                        (
                            Descricao, Tamanho, PrecoCusto, PrecoVenda, Origem, 
                            GrupoID, DataCompra, DataAlteracao, StatusId, 
                            MarcaId, GeneroID, PerfilID, Condicao
                        )
                        VALUES 
                        (
                            @Descricao, @Tamanho, @PrecoCusto, @PrecoVenda, @Origem, 
                            @GrupoId, NOW(), NOW(), @StatusId, 
                            @MarcaId, @GeneroId, @PerfilId, @Condicao
                        );
                        SELECT LAST_INSERT_ID();";

                    int produtoId = conn.QuerySingle<int>(sqlProduto, new
                    {
                        item.Descricao,
                        item.Tamanho,
                        item.PrecoCusto,
                        item.PrecoVenda,
                        item.Origem,
                        item.GrupoId,
                        StatusId = statusId,
                        item.MarcaId,
                        item.GeneroId,
                        item.PerfilId,
                        item.Condicao
                    }, trans);

                    // Insere o Estoque com o CodigoEstoque da sessão
                    var sqlEstoque = @"
                        INSERT INTO Estoque 
                        (Quantidade, Localizacao, DataAlteracao, ProdutoId, CodigoEstoque)
                        VALUES 
                        (1, 'Loja Principal', NOW(), @ProdutoId, @CodigoEstoque)";

                    conn.Execute(sqlEstoque, new { 
                        ProdutoId = produtoId, 
                        item.CodigoEstoque 
                    }, trans);
                }

                trans.Commit();
                Console.WriteLine($"[SESSÃO {sessaoId}] {sessao.Itens.Count} itens salvos no banco com sucesso!");
                return sessao.Itens.Count;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                Console.WriteLine($"[SESSÃO {sessaoId}] ERRO ao salvar no banco: {ex.Message}");
                throw new Exception($"Falha ao salvar no banco: {ex.Message}. Nenhum item foi gravado.");
            }
        }

        /// <summary>
        /// Lista sessões ativas (para debug)
        /// </summary>
        public List<object> ListarSessoes()
        {
            lock (_lock)
            {
                return _sessoes.Values.Select(s => new 
                {
                    s.Id,
                    s.Inicio,
                    s.OrigemFixa,
                    s.ProximoCodigo,
                    TotalItens = s.Itens.Count
                }).ToList<object>();
            }
        }
    }
}
