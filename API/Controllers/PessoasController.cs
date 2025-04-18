﻿using System;
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
    }    
}
