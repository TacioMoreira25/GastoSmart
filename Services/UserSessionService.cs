using GastoSmart.Models;

namespace GastoSmart.Services;

public class UserSessionService : IUserSessionService
{
    public PerfilUsuario? ActiveProfile { get; private set; }

    public bool IsLoggedIn => ActiveProfile != null;

    public void Login(PerfilUsuario profile)
    {
        ActiveProfile = profile;
    }

    public void Logout()
    {
        ActiveProfile = null;
    }
}
