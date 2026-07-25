using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastoSmart.Models;
using GastoSmart.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GastoSmart.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ILocalDatabaseService _dbService;
    private readonly IUserSessionService _sessionService;

    [ObservableProperty]
    private decimal saldoTotal;

    [ObservableProperty]
    private decimal gastosMes;

    [ObservableProperty]
    private bool isBusy;

    public ObservableCollection<Transacao> TransacoesRecentes { get; } = new();

    public DashboardViewModel(ILocalDatabaseService dbService, IUserSessionService sessionService)
    {
        _dbService = dbService;
        _sessionService = sessionService;
    }

    public async Task CarregarDadosAsync()
    {
        if (!_sessionService.IsLoggedIn) return;

        IsBusy = true;
        try
        {
            var perfilId = _sessionService.ActiveProfile!.Id;
            var transacoes = await _dbService.GetTransacoesPorPerfilAsync(perfilId);
            
            TransacoesRecentes.Clear();
            foreach (var t in transacoes)
            {
                TransacoesRecentes.Add(t);
            }

            SaldoTotal = transacoes.Sum(t => t.Valor);
            
            var mesAtual = DateTime.Now.Month;
            var anoAtual = DateTime.Now.Year;
            
            // Assuming negative values are expenses
            GastosMes = transacoes
                .Where(t => t.Valor < 0 && t.Data.Month == mesAtual && t.Data.Year == anoAtual)
                .Sum(t => t.Valor);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task NovaTransacao()
    {
        await Shell.Current.GoToAsync("NovaTransacaoPage");
    }
}
