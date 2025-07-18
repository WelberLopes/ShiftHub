using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebRegistro.Repository.Interfaces;
using WebRegistro.ViewModels;

[Authorize(Roles = "Admin,RH")]
public class DashboardController : Controller
{
    private readonly IUserRepository _userRepo;
    private readonly IBancoDeHorasRepository _bancoHorasRepo;

    public DashboardController(IUserRepository userRepo, IBancoDeHorasRepository bancoHorasRepo)
    {
        _userRepo = userRepo;
        _bancoHorasRepo = bancoHorasRepo;
    }

    public async Task<IActionResult> Index()
    {
        var todosFuncionarios =  _userRepo.GetAllUsers();
        var saldosIndividuais = new Dictionary<string, TimeSpan>();
        TimeSpan totalPositivo = TimeSpan.Zero;
        TimeSpan totalNegativo = TimeSpan.Zero;

        foreach (var funcionario in todosFuncionarios)
        {
            var saldo = await _bancoHorasRepo.CalcularSaldoAtualAsync(funcionario.Cpf);
            saldosIndividuais[funcionario.Cpf.ToString()] = saldo;

            if (saldo > TimeSpan.Zero)
                totalPositivo += saldo;
            else
                totalNegativo += saldo;
        }

        var viewModel = new RhDashboardViewModel
        {
            Funcionarios = todosFuncionarios,
            SaldosIndividuais = saldosIndividuais,
            TotalFuncionarios = todosFuncionarios.Count(),
            TotalHorasPositivas = totalPositivo,
            TotalHorasNegativas = totalNegativo
        };

        return View(viewModel);
    }
}