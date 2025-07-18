using WebRegistro.Models;

namespace WebRegistro.Repository.Interfaces;
    public interface IUserRepository
    {
    public IEnumerable<User> GetAllUsers();
    bool Create(User user);
    bool Update(User user);
    bool Delete(string idUser);
    User GetById(string idUser);
    public List<User> GetByCargo();
    bool VerifyExist(string idUser);
    Task<User> GetUser(string cpf);
    Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> cpfs);
    Task<IEnumerable<User>> GetAllUsersWithBiometricsAsync();
    User GetUserBiometricData(byte[]? biometricData);

}


