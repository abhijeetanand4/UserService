using UserService.Models;

namespace UserService.Services
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
    }
}
