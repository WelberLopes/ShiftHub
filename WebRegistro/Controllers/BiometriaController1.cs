using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

// Adicione os namespaces do seu projeto e do SDK
using DPFP;
using WebRegistro.Repository.Interfaces;

namespace WebRegistro.Controllers
{
    [Authorize]
    public class BiometriaController : Controller
    {
        private readonly IUserRepository _userRepo;

        public BiometriaController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        /// <summary>
        /// Action para exibir a página de cadastro de biometria para um funcionário.
        /// </summary>
        /// <returns>A View com a lista de funcionários.</returns>
        [HttpGet]
        public async Task<IActionResult> CadastrarFuncionario()
        {
            // A sua lógica para obter todos os funcionários do banco de dados
            var todosOsFuncionarios =  _userRepo.GetAllUsers();
            return View(todosOsFuncionarios);
        }

        /// <summary>
        /// Action para receber o template de uma nova digital (em Base64) e
        /// salvá-la no banco de dados para o utilizador especificado.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] BiometriaCadastroRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Cpf) || string.IsNullOrEmpty(request.BiometricData))
            {
                return Json(new { success = false, message = "Dados da requisição inválidos." });
            }

            try
            {
                // 1. Busca o utilizador. O DbContext começa a "trackear" esta instância.
                var utilizador = await _userRepo.GetUser(request.Cpf);
                if (utilizador == null)
                {
                    return Json(new { success = false, message = "Utilizador não encontrado." });
                }

                byte[] templateBytes;
                try
                {
                    templateBytes = Convert.FromBase64String(request.BiometricData);
                }
                catch (FormatException ex)
                {
                    return Json(new { success = false, message = $"Formato de dados biométricos inválido: {ex.Message}" });
                }
                var fincionariosComBio = await _userRepo.GetAllUsersWithBiometricsAsync();

                // Verifica se o utilizador já tem biometria cadastrada
                foreach (var funcionario in fincionariosComBio)
                {
                    if (funcionario.Cpf == request.Cpf && funcionario.BiometricTemplate != null)
                    {
                        return Json(new { success = false, message = "Biometria já cadastrada para este utilizador." });
                    }
                }
                // 2. Modifica a propriedade da instância que já está a ser trackeada.
                utilizador.BiometricTemplate = templateBytes;

                 _userRepo.Update(utilizador);

                return Json(new { success = true, message = "Biometria cadastrada com sucesso!" });
            }
            catch (Exception ex)
            {
                // Retorna a mensagem de erro específica para o frontend
                return Json(new { success = false, message = $"Ocorreu um erro no servidor: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// DTO para receber os dados do JSON enviado pelo frontend.
    /// </summary>
    public class BiometriaCadastroRequest
    {
        public string Cpf { get; set; }
        public string BiometricData { get; set; }
    }
}
