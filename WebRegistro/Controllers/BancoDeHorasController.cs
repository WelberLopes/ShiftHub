using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebRegistro.Repository.Interfaces;
using WebRegistro.ViewModels;
using WebRegistro.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using static WebRegistro.Models.BancoDeHoras;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WebRegistro.Controllers
{
    [Authorize] // Protege o controller inteiro, exigindo login.
    public class BancoDeHorasController : Controller
    {
        private readonly IBancoDeHorasRepository _bancoHorasRepo;
        private readonly IUserRepository _userRepo; // Supondo que você tenha um repositório de usuários

        public BancoDeHorasController(IBancoDeHorasRepository bancoHorasRepo, IUserRepository userRepo)
        {
            _bancoHorasRepo = bancoHorasRepo;
            _userRepo = userRepo;
        }

        /// <summary>
        /// Exibe o extrato completo de banco de horas para um usuário específico.
        /// </summary>
        /// <param name="id">O ID do usuário (funcionário).</param>
        [HttpGet]
        public async Task<IActionResult> Extrato(string id) // O 'id' aqui é o CPF
        {
            // Usamos o 'id' (CPF) para buscar o funcionário.
            var funcionario = await _userRepo.GetUser(id);
            if (funcionario == null)
            {
                return NotFound("Funcionário não encontrado.");
            }

            // Usamos o MESMO 'id' (CPF) para todas as chamadas, garantindo consistência.
            var movimentacoes = await _bancoHorasRepo.GetMovimentacoesPorUsuarioAsync(id);
            var saldoAtual = await _bancoHorasRepo.CalcularSaldoAtualAsync(id);

            // Cálculos feitos no controller, não na view
            var totalCreditos = movimentacoes
                .Where(m => m.TipoMovimentacao == TipoMovimentacaoHoras.Credito)
                .Aggregate(TimeSpan.Zero, (total, m) => total + m.Horas); // ou m.Duracao

            var totalDebitos = movimentacoes
                .Where(m => m.TipoMovimentacao == TipoMovimentacaoHoras.Debito)
                .Aggregate(TimeSpan.Zero, (total, m) => total + m.Horas); // ou m.Duracao

            var viewModel = new ExtratoViewModel
            {
                Funcionario = funcionario,
                Movimentacoes = movimentacoes,
                SaldoAtual = saldoAtual,
                TotalCreditos = totalCreditos,
                TotalDebitos = totalDebitos
            };

            return View(viewModel);
        }

        /// <summary>
        /// Exibe o formulário para adicionar uma nova movimentação manual.
        /// Acessível apenas para perfis de Admin ou RH.
        /// </summary>
        /// <param name="userId">O ID do usuário para quem a movimentação será adicionada.</param>
        [HttpGet]
        [Authorize(Roles = "Admin,RH")] // Apenas Admin e RH podem acessar
        public async Task<IActionResult> AdicionarMovimentacao(string userId)
        {
            var funcionario =  await _userRepo.GetUser(userId);
            if (funcionario == null)
            {
                return NotFound();
            }

            var viewModel = new AdicionarMovimentacaoViewModel
            {
                UserId = userId,
                NomeFuncionario = funcionario.NomeCompleto
            };

            return View(viewModel);
        }

        /// <summary>
        /// Processa o formulário de adição de movimentação.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,RH")]
        public async Task<IActionResult> AdicionarMovimentacao(AdicionarMovimentacaoViewModel viewModel)
        {
            TimeSpan duracao = TimeSpan.Zero;

            // --- SUGESTÃO DE MELHORIA AQUI ---
            if (!string.IsNullOrWhiteSpace(viewModel.DuracaoInput))
            {
                var partes = viewModel.DuracaoInput.Trim().Split(':');
                if (partes.Length == 2 && int.TryParse(partes[0], out int horas) && int.TryParse(partes[1], out int minutos))
                {
                    // Valida se os minutos estão entre 0 e 59
                    if (minutos >= 0 && minutos <= 59)
                    {
                        duracao = new TimeSpan(horas, minutos, 0);
                    }
                }
            }

            // Se a 'duracao' continuar como Zero, significa que o formato é inválido
            if (duracao == TimeSpan.Zero)
            {
                ModelState.AddModelError("DuracaoInput", "Formato inválido. Use HH:mm (ex: 08:30 ou 126:00).");
            }

            if (!ModelState.IsValid)
            {
                var funcionario = await _userRepo.GetUser(viewModel.UserId);
                if (funcionario != null)
                {
                    viewModel.NomeFuncionario = funcionario.NomeCompleto;
                }

                return View(viewModel);
            }

            // Agora a variável `duracao` está garantida aqui ✅
            var novaMovimentacao = new BancoDeHoras
            {
                UserId = viewModel.UserId,
                TipoMovimentacao = viewModel.TipoMovimentacao,
                Horas = duracao,
                Data = viewModel.DataRegistro,
                Descricao = viewModel.Descricao,
                DataRegistro = DateTime.Now
            };

            if (novaMovimentacao.TipoMovimentacao == TipoMovimentacaoHoras.Credito)
            {
                novaMovimentacao.DataExpiracao = novaMovimentacao.DataRegistro.AddMonths(6);
            }

            await _bancoHorasRepo.AdicionarMovimentacaoAsync(novaMovimentacao);

            TempData["SuccessMessage"] = "Movimentação registrada com sucesso!";
            return RedirectToAction("Extrato", new { id = viewModel.UserId });
        }
    }
            
        
    }
