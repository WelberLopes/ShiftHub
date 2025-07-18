using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using System.Runtime.Intrinsics.X86;
using System;
using WebRegistro.Models;
using WebRegistro.Repository;
using WebRegistro.Repository.Interfaces;
using WebRegistro.ViewModels;
using System.Threading.Tasks;

namespace WebRegistro.Controllers
{
    public class DepartamentoController : Controller
    {
        private readonly IDepartamentoRepository _departamentoRepository;
        private readonly IUserRepository _userRepository;
        public DepartamentoController(IDepartamentoRepository departamentoRepository, IUserRepository userRepository)
        {
            _departamentoRepository = departamentoRepository;
            _userRepository = userRepository;
        }
        // GET: DepartamentoController
        public async Task<IActionResult> Index()
        {
            try
            {
                var departamentos = await _departamentoRepository.GetAllDepartamentosAsync(); // Use a versão assíncrona
                if (departamentos == null || !departamentos.Any())
                {
                    ModelState.AddModelError("", "Nenhum departamento encontrado.");
                    return View(new List<Departamento>()); // Retorna uma lista vazia se não houver departamentos
                }
                return View(departamentos);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao verificar autenticação: " + ex.Message);
                return View(new List<Departamento>()); // Retorna uma lista vazia em caso de erro
            }
        }

        // GET: DepartamentoController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var departamento = await _departamentoRepository.GetDepartamentoByIdAsync(id); // Use a versão assíncrona
                if (departamento == null)
                {
                    return NotFound(); // Retorna erro 404 se o departamento não existir.
                }
                var  funcionarios = _departamentoRepository.GetFuncionariosByDepartamentoIdAsync(id).Result;
                var qtdFuncionario = funcionarios.Count();
                var viewModel = new DepartamentoDetailsViewModel { Departamento = departamento, Funcionarios = funcionarios, qtdFuncionario = qtdFuncionario };
                return View(viewModel);

            }
            catch (Exception ex)
            {
                var departamento = await _departamentoRepository.GetDepartamentoByIdAsync(id); // Use a versão assíncrona
                var  funcionarios = new List<User>(); // Lista vazia em caso de erro

                var qtdFuncionario = funcionarios.Count();
                var viewModel = new DepartamentoDetailsViewModel { Departamento = departamento, Funcionarios = funcionarios, qtdFuncionario = qtdFuncionario };
                return View(viewModel);
            }



        }
        private async Task PopularResponsavelDropdown()
        {
            var todosOsUsuarios =  _userRepository.GetAllUsers(); // Use a versão assíncrona
            ViewBag.Usuarios = new SelectList(todosOsUsuarios, "Cpf", "Nome");
        }
        // GET: DepartamentoController/Create
        // GET: DepartamentoController/Create

        // 1. O método GET agora é SÍNCRONO, pois não usa 'await'.
        public IActionResult Create()
        {
            // Chama o método síncrono, conforme sua restrição.
            var usuarios = _userRepository.GetAllUsers();

            var viewModel = new DepartamentoCreateViewModel
            {
                Departamento = new Departamento(),
                UsuariosDisponiveis = usuarios
            };

            return View(viewModel);
        }

        // POST: DepartamentoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 2. O método POST continua ASÍNCRONO, pois _departamentoRepository.AddDepartamentoAsync precisa de 'await'.
        public async Task<IActionResult> Create(DepartamentoCreateViewModel viewModel)
        {
            // 3. ADICIONAMOS UMA VALIDAÇÃO MANUAL PARA O DROPDOWN.
            // Isso garante que uma string vazia seja tratada como um erro.
            if (string.IsNullOrEmpty(viewModel.Departamento.ResponsavelCpf))
            {
                ModelState.AddModelError("Departamento.ResponsavelCpf", "É obrigatório selecionar um Responsável.");
            }

            if (!ModelState.IsValid)
            {
                // Repopula a lista com a chamada SÍNCRONA.
                viewModel.UsuariosDisponiveis = _userRepository.GetAllUsers();
                return View(viewModel);
            }

            try
            {
                // A chamada ao repositório de departamento continua com 'await', pois ela é assíncrona.
                await  _departamentoRepository.AddDepartamentoAsync(viewModel.Departamento);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao criar departamento: " + ex.Message);

                // Repopula a lista com a chamada SÍNCRONA em caso de erro.
                viewModel.UsuariosDisponiveis = _userRepository.GetAllUsers();
                return View(viewModel);
            }
        }

        // GET: DepartamentoController/Edit/5
        // GET: DepartamentoController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // 1. Busca o departamento específico que será editado.
            var departamento = await _departamentoRepository.GetDepartamentoByIdAsync(id);

            if (departamento == null)
            {
                return NotFound(); // Retorna erro 404 se o departamento não existir.
            }

            // 2. Busca a lista completa de usuários para o dropdown.
            var todosUsuarios =  _userRepository.GetAllUsers(); // Ou o método síncrono, se for o caso

            // 3. Cria a ViewModel e popula com os dados do departamento e a lista de usuários.
            var viewModel = new DepartamentoCreateViewModel
            {
                Departamento = departamento,
                UsuariosDisponiveis = todosUsuarios
            };

            // 4. Passa a ViewModel para a View.
            return View(viewModel);
        }

        // POST: DepartamentoController/Edit/5
        // POST: DepartamentoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // CORREÇÃO 1: A assinatura precisa ser 'async Task<IActionResult>' para usar await.
        public async Task<IActionResult> Edit(DepartamentoCreateViewModel viewModel)
        {
            // Adicionamos a validação manual para o dropdown, por segurança.
            if (string.IsNullOrEmpty(viewModel.Departamento.ResponsavelCpf))
            {
                ModelState.AddModelError("Departamento.ResponsavelCpf", "É obrigatório selecionar um Responsável.");
            }

            if (!ModelState.IsValid)
            {
                // CORREÇÃO 2: Se o modelo for inválido, precisamos repopular o dropdown
                // antes de retornar para a view, para evitar um erro de referência nula.
                viewModel.UsuariosDisponiveis = _userRepository.GetAllUsers(); // Usando a versão síncrona que você mencionou
                return View(viewModel);
            }

            try
            {
                // CORREÇÃO 3 (A MAIS CRÍTICA): Use 'await' para garantir que a operação
                // de atualização seja concluída antes de continuar.
                await _departamentoRepository.UpdateDepartamentoAsync(viewModel.Departamento);

                TempData["SuccessMessage"] = "Departamento atualizado com sucesso!"; // Mensagem de sucesso opcional
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao editar departamento: " + ex.Message);

                // CORREÇÃO 4: Repopule o dropdown aqui também em caso de erro.
                viewModel.UsuariosDisponiveis = _userRepository.GetAllUsers();
                return View(viewModel);
            }
        }

        // GET: DepartamentoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DepartamentoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
