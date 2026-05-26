using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoasController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PessoasController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/Pessoas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pessoa>>> GetPessoas()
        {
            return await _context.Pessoa.ToListAsync();
        }

        // GET: api/Pessoas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pessoa>> GetPessoa(int id)
        {
            var pessoa = await _context.Pessoa.FindAsync(id);

            if (pessoa == null)
            {
                return NotFound();
            }

            return pessoa;
        }

        // PUT: api/Pessoas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPessoa(int id, Pessoa pessoa)
        {
            if (id != pessoa.Id)
            {
                return BadRequest();
            }

            _context.Entry(pessoa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PessoaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Pessoas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Pessoa>> PostPessoa(Pessoa pessoa)
        {
            _context.Pessoa.Add(pessoa);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPessoa", new { id = pessoa.Id }, pessoa);
        }

        // DELETE: api/Pessoas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePessoa(int id)
        {
            var pessoa = await _context.Pessoa.FindAsync(id);
            if (pessoa == null)
            {
                return NotFound();
            }

            // Exclui os usuários associados a esta pessoa
            var linkedUsers = await _context.User.Where(u => u.PessoaId == id).ToListAsync();
            if (linkedUsers.Any())
            {
                _context.User.RemoveRange(linkedUsers);
            }

            // Exclui os endereços associados a esta pessoa
            var linkedEnderecos = await _context.Endereco.Where(e => e.PessoaID == id).ToListAsync();
            if (linkedEnderecos.Any())
            {
                _context.Endereco.RemoveRange(linkedEnderecos);
            }

            _context.Pessoa.Remove(pessoa);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PessoaExists(int id)
        {
            return _context.Pessoa.Any(e => e.Id == id);
        }

        [HttpGet("GetPessoaPorNick")]
        public ActionResult<Pessoa> GetPessoaPorNick(string nickName)
        {
            try
            {
                var pessoa = (from pes in _context.Pessoa
                              where pes.NickName == nickName
                              select pes).FirstOrDefault();



                if (pessoa == null)
                {
                    return new Pessoa() { NickName = "Cliente não encontrado" };
                }

                return pessoa;
            }
            catch (Exception)
            {

                return new Pessoa() { NickName = "Cliente não encontrado" };
            }

        }


        [HttpGet("GetPessoaPertence")]
        public IActionResult GetPessoaPertence()
        {
            var lstPessoa = (from pes in _context.Pessoa
                             where pes.Id >= 2 && pes.Id <= 4
                             select new
                             {
                                 pes.Id,
                                 pes.Nome
                             });

            return Ok(lstPessoa.ToList());
        }

        [HttpGet("GetPessoasGrid")]
        public IActionResult GetPessoasGrid()
        {
            // LINQ to SQL query joining Pessoa with PessoaGenero, PessoaCategoria, and PessoaStatus
            var query = from pessoa in _context.Pessoa
                        join genero in _context.PessoaGenero on pessoa.PessoaGeneroId equals genero.Id
                        join categoria in _context.PessoaCategoria on pessoa.PessoaCategoriaId equals categoria.Id
                        join status in _context.PessoaStatus on pessoa.StatusId equals status.Id
                        select new
                        {
                            // Campos da tabela Pessoa
                            Id = pessoa.Id,
                            Nome = pessoa.Nome,
                            CPF = pessoa.CPF, // CORREÇÃO: Selecionando CPF para exibir no grid
                            DataNascimento = pessoa.DataNascimento,
                            Email = pessoa.Email,
                            Telefone = pessoa.Telefone,
                            NickName = pessoa.NickName,

                            // Campos da tabela PessoaGenero
                            GeneroId = genero.Id,
                            GeneroDescricao = genero.Descricao,

                            // Campos da tabela PessoaCategoria
                            CategoriaId = categoria.Id,
                            CategoriaDescricao = categoria.Descricao,

                            // Campos da tabela PessoaStatus
                            StatusId = status.Id,
                            StatusDescricao = status.Descricao
                        };

            return Ok(query.ToList());
        }

        /// <summary>
        /// Retorna os dados de ID e Descrição da live
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetClientesCombo")]
        public IActionResult GetClientesCombo()
        {
            var clientes = from cli in _context.Pessoa
                           where cli.PessoaCategoriaId == 2
                           select new
                           {
                               cli.Id,
                               cli.Nome
                           };



            return Ok(clientes.ToList());
        }

        [HttpPost("RegistrarCliente")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> RegistrarCliente([FromBody] RegistrarClienteDto dto)
        {
            if (dto == null) return BadRequest("Dados inválidos.");
            if (string.IsNullOrWhiteSpace(dto.Nome)) return BadRequest("O nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("O e-mail é obrigatório.");
            if (string.IsNullOrWhiteSpace(dto.Telefone)) return BadRequest("O telefone é obrigatório.");
            if (string.IsNullOrWhiteSpace(dto.CPF)) return BadRequest("O CPF é obrigatório.");
            if (string.IsNullOrWhiteSpace(dto.NickName)) return BadRequest("O nick do Instagram é obrigatório.");

            // Limpa e valida CPF
            string cpfNumeros = new string(dto.CPF.Where(char.IsDigit).ToArray());
            if (!ValidaCPF(cpfNumeros))
            {
                return BadRequest("O CPF digitado não é válido.");
            }

            // Formata CPF
            string cpfFormatado = Convert.ToUInt64(cpfNumeros).ToString(@"000\.000\.000\-00");

            // Verifica se o CPF já está cadastrado
            bool cpfExiste = await _context.Pessoa.AnyAsync(p => p.CPF == cpfFormatado);
            if (cpfExiste)
            {
                return BadRequest("Este CPF já está cadastrado no sistema.");
            }

            // Verifica se o E-mail já está sendo usado por outro usuário
            bool emailExiste = await _context.User.AnyAsync(u => u.Email == dto.Email);
            if (emailExiste)
            {
                return BadRequest("Este e-mail já está cadastrado por outra cliente.");
            }

            // Busca dinamicamente os IDs padrão
            var categoria = await _context.PessoaCategoria.FirstOrDefaultAsync(c => c.Descricao == "Cliente")
                            ?? await _context.PessoaCategoria.FirstOrDefaultAsync(c => c.Id == 2)
                            ?? await _context.PessoaCategoria.FirstOrDefaultAsync();

            var tipo = await _context.PessoaTipo.FirstOrDefaultAsync(t => t.Descricao.Contains("Física") || t.Descricao.Contains("Fisica"))
                       ?? await _context.PessoaTipo.FirstOrDefaultAsync(t => t.Id == 1)
                       ?? await _context.PessoaTipo.FirstOrDefaultAsync();

            var status = await _context.PessoaStatus.FirstOrDefaultAsync(s => s.Descricao == "Ativo")
                         ?? await _context.PessoaStatus.FirstOrDefaultAsync(s => s.Id == 1)
                         ?? await _context.PessoaStatus.FirstOrDefaultAsync();

            var genero = await _context.PessoaGenero.FirstOrDefaultAsync(g => g.Descricao.Contains("Outro") || g.Descricao.Contains("Não"))
                         ?? await _context.PessoaGenero.FirstOrDefaultAsync();

            var tipoEndereco = await _context.TipoEndereco.FirstOrDefaultAsync(t => t.Descricao.Contains("Entrega") || t.Descricao.Contains("Residencial"))
                               ?? await _context.TipoEndereco.FirstOrDefaultAsync();

            if (categoria == null || tipo == null || status == null || genero == null)
            {
                return BadRequest("Erro na configuração dos parâmetros do sistema. Contate o administrador.");
            }

            // Garante o formato @ do Instagram
            string nickInstagram = dto.NickName.Trim();
            if (!nickInstagram.StartsWith("@")) nickInstagram = "@" + nickInstagram;

            // Cria Pessoa
            var pessoa = new Pessoa
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Telefone = new string(dto.Telefone.Where(char.IsDigit).ToArray()), // Apenas números para telefone
                CPF = cpfFormatado,
                NickName = nickInstagram,
                PessoaGeneroId = genero.Id,
                PessoaCategoriaId = categoria.Id,
                PessoaTipoId = tipo.Id,
                StatusId = status.Id,
                DataInclusao = DateTime.Now
            };

            _context.Pessoa.Add(pessoa);
            await _context.SaveChangesAsync();

            // Cria Endereço se as informações básicas de endereço forem enviadas
            if (!string.IsNullOrWhiteSpace(dto.CEP) && !string.IsNullOrWhiteSpace(dto.Logradouro))
            {
                var endereco = new Endereco
                {
                    PessoaID = pessoa.Id,
                    TipoEnderecoId = tipoEndereco?.Id ?? 1,
                    CEP = new string(dto.CEP.Where(char.IsDigit).ToArray()),
                    Logradouro = dto.Logradouro,
                    Unidade = dto.Numero ?? string.Empty, // Mapeia número do endereço para Unidade
                    Complemento = dto.Complemento,
                    Bairro = dto.Bairro ?? string.Empty,
                    Localidade = dto.Localidade ?? string.Empty, // Cidade
                    Estado = dto.Estado ?? string.Empty,
                    DataAlteracao = DateTime.Now
                };

                _context.Endereco.Add(endereco);
                await _context.SaveChangesAsync();
            }

            // GERAÇÃO AUTOMÁTICA DE USUÁRIO E SENHA ALEATÓRIA
            string senhaAleatoria = Guid.NewGuid().ToString().Substring(0, 8);
            var user = new User
            {
                Name = dto.Nome,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(senhaAleatoria), // Hash seguro
                IsActive = true,
                PessoaId = pessoa.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            // LOG DE EMAIL (Simulado no console para desenvolvimento / Envio SMTP real se configurado)
            Console.WriteLine($"[EMAIL PORTAL CLIENTE] Destinatário: {dto.Email}");
            Console.WriteLine($"Assunto: Seu acesso ao portal A Brechozeira");
            Console.WriteLine($"Mensagem: Olá {dto.Nome}, seu cadastro foi finalizado! Use seu email ({dto.Email}) e senha ({senhaAleatoria}) para gerenciar seu cadastro.");

            return Ok(new 
            { 
                success = true, 
                message = "Cliente e usuário registrados com sucesso!", 
                clienteId = pessoa.Id,
                email = dto.Email,
                password = senhaAleatoria
            });
        }

        [HttpGet("ObterDadosPerfil")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ObterDadosPerfil()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Acesso não autorizado.");
            }

            var user = await _context.User
                .Include(u => u.Pessoa)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Pessoa == null)
            {
                return NotFound("Ficha cadastral não encontrada.");
            }

            var endereco = await _context.Endereco
                .FirstOrDefaultAsync(e => e.PessoaID == user.PessoaId);

            return Ok(new
            {
                id = user.Pessoa.Id,
                nome = user.Pessoa.Nome,
                email = user.Pessoa.Email,
                telefone = user.Pessoa.Telefone,
                cpf = user.Pessoa.CPF,
                nickName = user.Pessoa.NickName,
                
                // Endereço
                cep = endereco?.CEP,
                logradouro = endereco?.Logradouro,
                numero = endereco?.Unidade,
                complemento = endereco?.Complemento,
                bairro = endereco?.Bairro,
                localidade = endereco?.Localidade,
                estado = endereco?.Estado
            });
        }

        [HttpPost("AtualizarPerfil")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> AtualizarPerfil([FromBody] RegistrarClienteDto dto)
        {
            if (dto == null) return BadRequest("Dados inválidos.");
            if (string.IsNullOrWhiteSpace(dto.Nome)) return BadRequest("O nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(dto.Telefone)) return BadRequest("O telefone é obrigatório.");
            if (string.IsNullOrWhiteSpace(dto.NickName)) return BadRequest("O nick do Instagram é obrigatório.");

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Acesso não autorizado.");
            }

            var user = await _context.User
                .Include(u => u.Pessoa)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Pessoa == null)
            {
                return NotFound("Ficha cadastral não encontrada.");
            }

            // Normaliza telefone
            string telefoneLimpo = new string(dto.Telefone.Where(char.IsDigit).ToArray());

            // Se o e-mail mudou, verifica unicidade no User e Pessoa
            if (dto.Email != user.Pessoa.Email)
            {
                if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("O e-mail é obrigatório.");
                bool emailExiste = await _context.User.AnyAsync(u => u.Email == dto.Email && u.Id != user.Id);
                if (emailExiste)
                {
                    return BadRequest("Este e-mail já está sendo utilizado por outro usuário.");
                }
                user.Email = dto.Email;
                user.Pessoa.Email = dto.Email;
            }

            // Atualiza campos da Pessoa
            user.Pessoa.Nome = dto.Nome;
            user.Pessoa.Telefone = telefoneLimpo;
            
            // Garante o formato @ do Instagram
            string nickInstagram = dto.NickName.Trim();
            if (!nickInstagram.StartsWith("@")) nickInstagram = "@" + nickInstagram;
            user.Pessoa.NickName = nickInstagram;

            user.Name = dto.Nome; // Sincroniza nome do usuário

            // Atualiza ou Cria Endereço
            var endereco = await _context.Endereco.FirstOrDefaultAsync(e => e.PessoaID == user.PessoaId);
            if (!string.IsNullOrWhiteSpace(dto.CEP) && !string.IsNullOrWhiteSpace(dto.Logradouro))
            {
                if (endereco == null)
                {
                    var tipoEndereco = await _context.TipoEndereco.FirstOrDefaultAsync(t => t.Descricao.Contains("Entrega") || t.Descricao.Contains("Residencial"))
                                       ?? await _context.TipoEndereco.FirstOrDefaultAsync();

                    endereco = new Endereco
                    {
                        PessoaID = user.PessoaId,
                        TipoEnderecoId = tipoEndereco?.Id ?? 1,
                        CEP = new string(dto.CEP.Where(char.IsDigit).ToArray()),
                        Logradouro = dto.Logradouro,
                        Unidade = dto.Numero ?? string.Empty,
                        Complemento = dto.Complemento,
                        Bairro = dto.Bairro ?? string.Empty,
                        Localidade = dto.Localidade ?? string.Empty,
                        Estado = dto.Estado ?? string.Empty,
                        DataAlteracao = DateTime.Now
                    };
                    _context.Endereco.Add(endereco);
                }
                else
                {
                    endereco.CEP = new string(dto.CEP.Where(char.IsDigit).ToArray());
                    endereco.Logradouro = dto.Logradouro;
                    endereco.Unidade = dto.Numero ?? string.Empty;
                    endereco.Complemento = dto.Complemento;
                    endereco.Bairro = dto.Bairro ?? string.Empty;
                    endereco.Localidade = dto.Localidade ?? string.Empty;
                    endereco.Estado = dto.Estado ?? string.Empty;
                    endereco.DataAlteracao = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Seus dados foram atualizados com sucesso!" });
        }

        private static bool ValidaCPF(string cpf)
        {
            if (string.IsNullOrEmpty(cpf)) return false;
            cpf = new string(cpf.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11) return false;

            // Evita CPFs conhecidos inválidos
            if (cpf.Distinct().Count() == 1) return false;

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }
    }

    public class RegistrarClienteDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; // Obrigatório para o Login do portal
        public string Telefone { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty; // Nick do Instagram obrigatório
        public string? CEP { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Estado { get; set; }
    }
}
