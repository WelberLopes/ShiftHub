using Microsoft.EntityFrameworkCore;
using WebRegistro.Data;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;

namespace WebRegistro.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly ApplicationDbContext _context;

        public LoginRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public User GetUser(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Cpf == id);
            return user;
        }

        public bool VerifyPassword(string id, string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword))
                return false;
            var user = _context.Users.FirstOrDefault(u => u.Cpf == id);
            if (user == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(plainPassword, user.PasswordHash);
        }
        public async Task<IEnumerable<User>> GetAllUsersWithBiometrics()
        {
            return await _context.Users.Where(u => u.BiometricTemplate != null).ToListAsync();
        }
    }
}
