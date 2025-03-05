using BoardGameBackend.Models;

namespace BoardGameBackend.Repositories
{
    public interface IAuthService
    {
        Task<Exception?> SignUpAsync(UserModel userModel);
        Task<AuthenticateData?> LoginAsync(LoginUserDto userLogin);
        Task<UserModel> GetUserById(Guid id);
    }
}