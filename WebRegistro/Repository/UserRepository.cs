using Microsoft.EntityFrameworkCore;
using WebRegistro.Data;
using WebRegistro.Models;
using WebRegistro.Repository.Interfaces;
using BCrypt.Net;

namespace WebRegistro.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context, IServiceProvider service)
        {
            _context = context;
        }
        bool IUserRepository.Create(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
                return false;
            }

            User newUser = new User() { NomeCompleto = user.NomeCompleto, DepartamentoId = user.DepartamentoId, Cpf = user.Cpf, DataAdmissao = user.DataAdmissao, Email = user.Email, Cargo = user.Cargo, Role = user.Role, PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash) }; 

            _context.Users.Add(newUser);
            _context.SaveChanges();
            return true;
        }

        bool IUserRepository.Delete(string idUser)
        {
            var user = _context.Users.Find(idUser);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                return true; // Usuário deletado com sucesso
            }
            return false;
            throw new Exception("Usuário não encontrado");
        }

        IEnumerable<User> IUserRepository.GetAllUsers()
        {
            var users = _context.Users.ToList();
            if (users == null || !users.Any())
            {
                return new List<User>(); // Retorna uma lista vazia se não houver usuários
            }
            return users;
        }


        User IUserRepository.GetById(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
            {
                throw new ArgumentException("ID de usuário não pode ser nulo ou vazio.");
            }
            var user = _context.Users.FirstOrDefault(u => u.Cpf == cpf.ToString());
            if (user == null)
            {
                throw new Exception("Usuário não encontrado");
            }
            return user; // Retorna o usuário encontrado
        }

        bool IUserRepository.Update(User user)
        {
            /* var userToUpdate = new User()
             {
                 Cpf = user.Cpf,
                 NomeCompleto = user.NomeCompleto,
                 Email = user.Email,
                 Cargo = user.Cargo,
                 DataAdmissao = user.DataAdmissao,
                 Role = user.Role,
                 PasswordHash = user.PasswordHash,
                 BiometricTemplate = user.BiometricTemplate ?? null // Permite que o campo BiometricTemplate seja nulo se não for fornecido
             };
             if (userToUpdate == null)
             {
                 throw new ArgumentNullException(nameof(userToUpdate));
             }
             var existingUser = _context.Users.FirstOrDefault(u => u.Cpf == userToUpdate.Cpf);
             if (existingUser == null)
             {
                 throw new Exception("Usuário não encontrado");
             }
             _context.Entry(userToUpdate).State = EntityState.Modified;*/
            _context.Users.Update(user);
            _context.SaveChanges();
            return true;

        }
        bool IUserRepository.VerifyExist(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
            {
                return true; // Considerar que se o idUser for nulo ou vazio, não existe usuário
                throw new ArgumentNullException(nameof(cpf));
            }
            if (_context.Users.Any(u => u.Cpf == cpf) == true)
            {
                return true; // O usuário existe
            }
            else
            {
                return false; // O usuário não existe
            }

        }
        List<User> IUserRepository.GetByCargo()
        {
            var users = _context.Users.Where(u => u.Cargo == "Técnico de Enfermagem").ToList();
            if (users == null || !users.Any())
            {
                return new List<User>(); // Retorna uma lista vazia se não houver usuários
            }
            return users; // Retorna os usuários agrupados por cargo
        }
        async Task<User> IUserRepository.GetUser(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
            {
                throw new ArgumentException("CPF não pode ser nulo ou vazio.");
            }
            var user = _context.Users.FirstOrDefault(u => u.Cpf == cpf);
            if (user == null)
            {
                throw new Exception("Usuário não encontrado");
            }
            return user; // Retorna o usuário encontrado
        }
        public async Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> cpfs)
        {
            // Usa o método Where e Contains para buscar todos os usuários cujos CPFs estão na lista
            return await _context.Users.Where(u => cpfs.Contains(u.Cpf)).ToListAsync();
        }

        User IUserRepository.GetUserBiometricData(byte[]? biometricData)
        {
            var user = _context.Users.FirstOrDefault(u => u.BiometricTemplate != null && u.BiometricTemplate.SequenceEqual(biometricData));
            if (user == null)
            {
                throw new Exception("Usuário com dados biométricos não encontrado");
            }
            return user; // Retorna o usuário encontrado com dados biométricos correspondentes
        }
        public async Task<IEnumerable<User>> GetAllUsersWithBiometricsAsync()
        {
            // Busca todos os usuários que possuem dados biométricos
            return await _context.Users.Where(u => u.BiometricTemplate != null).ToListAsync();
        }
    }
}

