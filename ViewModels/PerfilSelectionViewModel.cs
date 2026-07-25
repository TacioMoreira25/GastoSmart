using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastoSmart.Models;
using GastoSmart.Services;
using GastoSmart.Utils;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GastoSmart.ViewModels;

public partial class PerfilSelectionViewModel : ObservableObject
{
    private readonly ILocalDatabaseService _dbService;
    private readonly IUserSessionService _sessionService;

    [ObservableProperty]
    private ObservableCollection<PerfilUsuario> perfis = new();

    public PerfilSelectionViewModel(ILocalDatabaseService dbService, IUserSessionService sessionService)
    {
        _dbService = dbService;
        _sessionService = sessionService;
    }

    public async Task LoadPerfisAsync()
    {
        var list = await _dbService.GetPerfisAsync();
        Perfis.Clear();
        foreach (var p in list)
        {
            Perfis.Add(p);
        }
    }

    [RelayCommand]
    private async Task CriarPerfilAsync()
    {
        string nome = await Shell.Current.DisplayPromptAsync("Novo Perfil", "Digite o nome do perfil:");
        if (string.IsNullOrWhiteSpace(nome)) return;

        string pin = await Shell.Current.DisplayPromptAsync("Segurança", "Digite um PIN de 4 dígitos para este perfil:", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
        {
            await Shell.Current.DisplayAlertAsync("Erro", "O PIN deve ter no mínimo 4 dígitos.", "OK");
            return;
        }

        var novoPerfil = new PerfilUsuario
        {
            Nome = nome,
            SenhaPinHash = CryptoUtils.HashPin(pin),
            DataCriacao = System.DateTime.Now,
            Cor = "#512BD4"
        };

        await _dbService.SavePerfilAsync(novoPerfil);
        await LoadPerfisAsync();
    }

    [RelayCommand]
    private async Task SelecionarPerfilAsync(PerfilUsuario perfil)
    {
        if (perfil == null) return;

        string pin = await Shell.Current.DisplayPromptAsync("Acesso", $"Digite o PIN para {perfil.Nome}:", keyboard: Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(pin)) return;

        var hashDigitado = CryptoUtils.HashPin(pin);
        if (hashDigitado == perfil.SenhaPinHash)
        {
            _sessionService.Login(perfil);
            // Navigate to Dashboard
            await Shell.Current.GoToAsync("//DashboardPage");
        }
        else
        {
            await Shell.Current.DisplayAlertAsync("Erro", "PIN incorreto.", "OK");
        }
    }
}
