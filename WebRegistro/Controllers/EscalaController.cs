using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;
using WebRegistro.ViewModels;

namespace WebRegistro.Controllers
{
    [Authorize]
    public class EscalaController : Controller
    {
   
        private readonly IUserRepository _userRepo;
        private readonly IEscalaRepository _escalaRepo;

        public EscalaController(IEscalaRepository escalaRepo, IUserRepository userRepo)
        {

            _escalaRepo = escalaRepo;
            _userRepo = userRepo;
        }

        public async Task<IActionResult> Index(string? competencia, int? unidadeId)
        {
            if (!DateTime.TryParse(competencia + "-01", out var dataReferencia))
            {
                dataReferencia = DateTime.Now;
            }

            int unidadeAtual = unidadeId.GetValueOrDefault(0);

            string nomeUnidadeAtual;
            switch (unidadeAtual)
            {
                case 1:
                    nomeUnidadeAtual = "Hba";
                    break;
                case 2:
                    nomeUnidadeAtual = "Multiclin";
                    break;
                case 3:
                    nomeUnidadeAtual = "BiocheckUp";
                    break;
                default:
                    nomeUnidadeAtual = "Todas as Unidades";
                    break;
            }

            IEnumerable<User> funcionariosParaExibir;
            IEnumerable<Escala> escalasParaProcessar;

            if (unidadeAtual == 0)
            {
                // --- LÓGICA PARA "TODAS AS UNIDADES" ---

                // 1. Busca todos os funcionários que podem ter escala.
                funcionariosParaExibir = _userRepo.GetByCargo().OrderBy(r=> r.NomeCompleto);

                // 2. Busca todas as escalas do mês.
                escalasParaProcessar = await _escalaRepo.GetEscalasDoMesAsync(dataReferencia.Year, dataReferencia.Month);
            }
            else
            {
                // --- LÓGICA PARA UMA UNIDADE ESPECÍFICA ---

                // 1. Busca primeiro apenas as escalas da unidade selecionada.
                var escalasDaUnidade = await _escalaRepo.GetEscalaPorUnidade(unidadeAtual, dataReferencia.Year, dataReferencia.Month);
                escalasParaProcessar = escalasDaUnidade; // A lista de escalas a ser processada é esta.

                // 2. Extrai a lista de IDs (CPFs) únicos dos funcionários que têm escalas nesta unidade.
                var idsDosFuncionariosNaUnidade = escalasDaUnidade.Select(e => e.FuncionarioId).Distinct().ToList();

                // 3. Busca os dados completos apenas desses funcionários.
                if (idsDosFuncionariosNaUnidade.Any())
                {
                    // Usa o novo método do repositório.
                    funcionariosParaExibir = await _userRepo.GetUsersByIdsAsync(idsDosFuncionariosNaUnidade);
                }
                else
                {
                    // Se não houver escalas, não há funcionários para exibir.
                    funcionariosParaExibir = new List<User>();
                }
            }

            // --- MONTAGEM DO VIEWMODEL (Lógica Comum) ---

            var viewModel = new GestaoEscalaViewModel
            {
                DataReferencia = dataReferencia,
                Funcionarios = funcionariosParaExibir.ToList(), // A lista de funcionários agora é dinâmica!
                Unidade = unidadeAtual,
                Escalas = new Dictionary<string, Dictionary<int, Escala>>(),
                NomeUnidade = nomeUnidadeAtual,
            };

            // Inicializa o dicionário para todos os funcionários que serão exibidos.
            foreach (var func in funcionariosParaExibir)
            {
                if (!viewModel.Escalas.ContainsKey(func.Cpf))
                {
                    viewModel.Escalas[func.Cpf] = new Dictionary<int, Escala>();
                }
            }

            // Preenche o dicionário com as escalas encontradas.
            foreach (var escala in escalasParaProcessar)
            {
                if (viewModel.Escalas.ContainsKey(escala.FuncionarioId))
                {
                    viewModel.Escalas[escala.FuncionarioId][escala.Data.Day] = escala;
                }
            }

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Enfermagem")]
        public async Task<IActionResult> DefinirEscala(string funcionarioId, DateTime data, string tipo, string turno, string tipoExame, TimeSpan? horaInicio, TimeSpan? horaFim, int unidadeId)
        {
            var escala = new Escala
            {
                FuncionarioId = funcionarioId,
                Data = data,
                Tipo = tipo,
                Turno = tipo == "Trabalho" ? turno : null,
                TipoExame = tipo == "Trabalho" ? tipoExame : null,
                HoraInicio = tipo == "Trabalho" ? horaInicio : null,
                HoraFim = tipo == "Trabalho" ? horaFim : null,
                Unidade = unidadeId
            };

            await _escalaRepo.AdicionarOuAtualizarEscalaAsync(escala);
            return RedirectToAction("Index", new { competencia = data.ToString("yyyy-MM"), unidadeId = unidadeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Enfermagem")]
        public async Task<IActionResult> RemoverEscala(int escalaId)
        {
            var escala = await _escalaRepo.GetEscalaAsync(escalaId);
            if (escala != null)
            {
                await _escalaRepo.RemoverEscalaAsync(escala);
                return RedirectToAction("Index", new { competencia = escala.Data.ToString("yyyy-MM") });
            }
            return NotFound();
        }
    }
}
