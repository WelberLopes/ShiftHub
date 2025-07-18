using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebRegistro.Models;

namespace WebRegistro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Construtor simplificado sem o SignInManager
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Verifica se o utilizador está autenticado
            if (User.Identity.IsAuthenticated)
            {
                // Se estiver, redireciona para a página principal da aplicação (o painel de ponto)
                return RedirectToAction("Index", "Ponto");
            }

            // Se não estiver, redireciona para a página de login do Identity
            return Redirect("Login");
        }
    }
}