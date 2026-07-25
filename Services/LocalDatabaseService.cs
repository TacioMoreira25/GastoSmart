using SQLite;
using GastoSmart.Models;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace GastoSmart.Services;

public class LocalDatabaseService : ILocalDatabaseService
{
    private SQLiteAsyncConnection? _database;
    private const string DatabaseFilename = "GastoSmartSQLite.db3";
    private readonly string _databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    public async Task InicializarBancoAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(_databasePath);
        
        await _database.CreateTableAsync<PerfilUsuario>();
        await _database.CreateTableAsync<Transacao>();
    }

    public async Task<List<PerfilUsuario>> GetPerfisAsync()
    {
        await InicializarBancoAsync();
        return await _database!.Table<PerfilUsuario>().ToListAsync();
    }

    public async Task<PerfilUsuario?> GetPerfilAsync(int id)
    {
        await InicializarBancoAsync();
        return await _database!.Table<PerfilUsuario>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SavePerfilAsync(PerfilUsuario perfil)
    {
        await InicializarBancoAsync();
        if (perfil.Id != 0)
            return await _database!.UpdateAsync(perfil);
        else
            return await _database!.InsertAsync(perfil);
    }

    public async Task<int> DeletePerfilAsync(PerfilUsuario perfil)
    {
        await InicializarBancoAsync();
        // Delete all transactions from this profile as well
        await _database!.Table<Transacao>().Where(t => t.PerfilId == perfil.Id).DeleteAsync();
        return await _database.DeleteAsync(perfil);
    }

    public async Task<List<Transacao>> GetTransacoesPorPerfilAsync(int perfilId)
    {
        await InicializarBancoAsync();
        return await _database!.Table<Transacao>().Where(t => t.PerfilId == perfilId).OrderByDescending(t => t.Data).ToListAsync();
    }

    public async Task<int> SaveTransacaoAsync(Transacao transacao)
    {
        await InicializarBancoAsync();
        if (transacao.Id != 0)
            return await _database!.UpdateAsync(transacao);
        else
            return await _database!.InsertAsync(transacao);
    }

    public async Task<int> DeleteTransacaoAsync(Transacao transacao)
    {
        await InicializarBancoAsync();
        return await _database!.DeleteAsync(transacao);
    }
}
