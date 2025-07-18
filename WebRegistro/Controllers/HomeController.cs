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
            // Verifica se o utilizador est� autenticado
            if (User.Identity.IsAuthenticated)
            {
                // Se estiver, redireciona para a p�gina principal da aplica��o (o painel de ponto)
                return RedirectToAction("Index", "Ponto");
            }

            // Se n�o estiver, redireciona para a p�gina de login do Identity
            return Redirect("Login");
        }
    }
}