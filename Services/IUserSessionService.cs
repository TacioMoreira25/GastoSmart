using GastoSmart.Models;

namespace GastoSmart.Services;

public interface IUserSessionService
{
    PerfilUsuario? ActiveProfile { get; }
    void Login(PerfilUsuario profile);
    void Logout();
    bool IsLoggedIn { get; }
}
