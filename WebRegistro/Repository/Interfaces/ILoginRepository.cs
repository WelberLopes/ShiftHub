using WebRegistro.Models;

namespace WebRegistro.Repository.Interfaces
{
    public interface ILoginRepository 
    {
        bool VerifyPassword(string cpf, string hashPassword);
        User GetUser(string cpf);
        public Task<IEnumerable<User>> GetAllUsersWithBiometrics();

    }
}
