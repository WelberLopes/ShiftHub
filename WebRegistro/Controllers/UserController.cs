using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebRegistro.Data;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;
using WebRegistro.ViewModels;

namespace WebRegistro.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;
        private readonly IDepartamentoRepository _departamentoRepository;
        public UserController(IUserRepository userRepository, ApplicationDbContext context, IDepartamentoRepository departamentoRepository)
        {
            _userRepository = userRepository;
            _context = context;
            _departamentoRepository = departamentoRepository;
        }
        public IActionResult Index()
        {
            
            return View("Create");
        }
        [Route("User/Details/{id}")]
        public IActionResult Details(string cpf)
        {
            var user = _userRepository.GetById(cpf);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        // GET: /User/Create
        public async Task<IActionResult> Create()
        {
            // Use async/await para não bloquear a thread
            var departamentos = await _departamentoRepository.GetAllDepartamentosAsync();

            var viewModel = new UserCreateViewModel
            {
                Usuario = new User(),
                DepartamentosDisponiveis = departamentos
            };

            return View(viewModel);
        }

        // POST: /User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel userViewModel) // Renomeado para clareza
        {
            // A validação do ModelState já é verificada aqui
            if (!ModelState.IsValid)
            {
                // Se o modelo for inválido, recarregue os departamentos antes de retornar à view
                userViewModel.DepartamentosDisponiveis = await _departamentoRepository.GetAllDepartamentosAsync();
                return View(userViewModel);
            }

            if (_userRepository.VerifyExist(userViewModel.Usuario.Cpf))
            {
                ModelState.AddModelError("Usuario.Cpf", "Já existe um usuário com este CPF.");
                // Recarregue os departamentos também em caso de erro de negócio
                userViewModel.DepartamentosDisponiveis = await _departamentoRepository.GetAllDepartamentosAsync();
                return View(userViewModel);
            }

            try
            {
                // O código duplicado foi removido daqui

                _userRepository.Create(userViewModel.Usuario);
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocorreu um erro inesperado ao criar o usuário: " + ex.Message);
                // Recarregue os departamentos em caso de exceção
                userViewModel.DepartamentosDisponiveis = await _departamentoRepository.GetAllDepartamentosAsync();
                return View(userViewModel);
            }
        }
        // GET: User/Edit/12345678900
        [HttpGet]
        public async Task<IActionResult> Edit(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
            {
                return BadRequest("CPF não pode ser nulo.");
            }

            User user = _userRepository.GetById(cpf);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserCreateViewModel
            {
                Usuario = user,
                // Use await para obter o resultado da tarefa de forma assíncrona
                DepartamentosDisponiveis = await _departamentoRepository.GetAllDepartamentosAsync()
            };

            return View(viewModel);
        }

        // POST: User/Edit/12345678900
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string cpf, UserCreateViewModel viewModel)
        {
            if (cpf != viewModel.Usuario.Cpf)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                // Se o modelo for inválido, recarregue os dados necessários para a View
                viewModel.DepartamentosDisponiveis = await _departamentoRepository.GetAllDepartamentosAsync();
                return View(viewModel); // Retorna à view de edição com os erros
            }

            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Cpf == viewModel.Usuario.Cpf);

                if (existingUser == null)
                {
                    return NotFound();
                }

                // Atualize os campos do usuário existente com os dados do ViewModel
                existingUser.NomeCompleto = viewModel.Usuario.NomeCompleto;
                existingUser.Email = viewModel.Usuario.Email;
                existingUser.Cargo = viewModel.Usuario.Cargo;
                existingUser.Role = viewModel.Usuario.Role;
                existingUser.DataAdmissao = viewModel.Usuario.DataAdmissao;
                existingUser.DepartamentoId = viewModel.Usuario.DepartamentoId; // Não se esqueça de atualizar a chave estrangeira

                // Atualiza a senha APENAS se um novo valor foi fornecido
                if (!string.IsNullOrEmpty(viewModel.Usuario.PasswordHash))
                {
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(viewModel.Usuario.PasswordHash);
                }

                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Admin");
            }
            catch (DbUpdateConcurrencyException)
            {
                // Adicionar tratamento para concorrência se necessário
                ModelState.AddModelError("", "Ocorreu um erro ao salvar. O usuário pode ter sido modificado por outra pessoa.");
                viewModel.DepartamentosDisponiveis = await _departamentoRepository.GetAllDepartamentosAsync();
                return View(viewModel);
            }
        }
        public IActionResult Delete(string cpf)
        {
            var user = _userRepository.GetById(cpf);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string idUser)
        {
            var user = _userRepository.GetById(idUser);
            if (user == null)
            {
                return NotFound();
            }

            try
            {

                _userRepository.Delete(idUser);
                TempData["SuccessMessage"] = "Usuário excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao excluir Usuário: " + ex.Message;
                return View(user);
            }
        }

    }
}
