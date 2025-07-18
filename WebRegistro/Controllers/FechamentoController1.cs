using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebRegistro.Repository;
using WebRegistro.Repository.Interfaces;
using WebRegistro.Services.Interfaces;

namespace WebRegistro.Controllers
{
    [Authorize(Roles = "Admin,RH, Administrador")]
    public class FechamentoController : Controller
    {
        private readonly IFechamentoMensalService _fechamentoService;
        private readonly IBancoDeHorasRepository _bancoHorasRepo;
        public FechamentoController(IFechamentoMensalService fechamentoService,IBancoDeHorasRepository bancoDeHorasRepository)
        {
            _fechamentoService = fechamentoService;
            _bancoHorasRepo = bancoDeHorasRepository;
        }

        // Action para exibir a página com o botão
        [HttpGet]
        public IActionResult Index()
        {
            // Passa a competência do mês anterior como sugestão
            ViewBag.CompetenciaSugerida = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
            return View();
        }

        // Action que será chamada pelo botão para executar o processo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecutarFechamento(string competencia)
        {
            if (string.IsNullOrEmpty(competencia) || !DateTime.TryParse(competencia + "-01", out var dataCompetencia))
            {
                TempData["ErrorMessage"] = "Competência inválida. Por favor, selecione um mês e ano.";
                return RedirectToAction("Index");
            }
            var list = await _bancoHorasRepo.GetAllMovimentacoesPeriodo(dataCompetencia);

            if(list != null && list.ToList().Count > 0)
            {
                TempData["ErrorMessage"] = "Já existe movimentações para a competência selecionada. Fechamento não pode ser realizado.";
                return RedirectToAction("Index");
            }

            try
            {
                var resultado = await _fechamentoService.ExecutarFechamentoAsync(dataCompetencia.Year, dataCompetencia.Month);
                TempData["SuccessMessage"] = $"Fechamento da competência {competencia} iniciado. {resultado}";
            }
            catch (Exception ex)
            {
                // Idealmente, logar o erro ex
                TempData["ErrorMessage"] = "Ocorreu um erro inesperado durante o fechamento: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}