using GastoSmart.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GastoSmart.Services;

public interface ILocalDatabaseService
{
    Task InicializarBancoAsync();
    
    // Perfil
    Task<List<PerfilUsuario>> GetPerfisAsync();
    Task<PerfilUsuario?> GetPerfilAsync(int id);
    Task<int> SavePerfilAsync(PerfilUsuario perfil);
    Task<int> DeletePerfilAsync(PerfilUsuario perfil);
    
    // Transacao
    Task<List<Transacao>> GetTransacoesPorPerfilAsync(int perfilId);
    Task<int> SaveTransacaoAsync(Transacao transacao);
    Task<int> DeleteTransacaoAsync(Transacao transacao);
}
