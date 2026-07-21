using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gastosmart.App.DTOs;
using GastoSmart.App.Services;

using System.Text.Json.Serialization;

namespace GastoSmart.App.ViewModels;

public class Transacao
{
    [JsonPropertyName("title")]
    public string Descricao { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Valor { get; set; }
    
    [JsonPropertyName("date")]
    public DateTime Data { get; set; }
    
    [JsonPropertyName("categoryName")]
    public string Tipo { get; set; } = string.Empty;
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    public partial decimal SaldoTotal { get; set; }

    [ObservableProperty]
    public partial decimal GastosMes { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public ObservableCollection<Transacao> TransacoesRecentes { get; } = new();

    public DashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task CarregarDadosAsync()
    {
        IsBusy = true;
        try
        {
            var transacoes = await _apiService.GetTransacoesAsync();
            TransacoesRecentes.Clear();
            
            decimal saldo = 0;
            decimal gastosMes = 0;
            var mesAtual = DateTime.Now.Month;
            var anoAtual = DateTime.Now.Year;

            if (transacoes != null)
            {
                foreach (var t in transacoes)
                {
                    TransacoesRecentes.Add(t);
                    
                    if (t.Tipo?.Equals("Receita", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        saldo += t.Valor;
                    }
                    else
                    {
                        saldo -= t.Valor;
                        
                        if (t.Data.Month == mesAtual && t.Data.Year == anoAtual)
                        {
                            gastosMes += t.Valor;
                        }
                    }
                }
            }

            SaldoTotal = saldo;
            GastosMes = gastosMes;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Erro ao carregar dados: {ex.Message}");
            if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page != null)
            {
                await Application.Current.Windows[0].Page!.DisplayAlertAsync("Erro de Conexão", $"Não foi possível carregar os dados do painel: {ex.Message}", "OK");
            }
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

    [RelayCommand]
    public async Task ScanReceiptAsync()
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    IsBusy = true;
                    var transactionData = await _apiService.ScanReceiptAsync(photo);
                    
                    if (transactionData != null)
                    {
                        var navigationParameter = new Dictionary<string, object>
                        {
                            { "TransactionData", transactionData }
                        };
                        await Shell.Current.GoToAsync("SaveTransactionPage", navigationParameter);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (Application.Current?.Windows[0].Page != null)
            {
                await Application.Current.Windows[0].Page?.DisplayAlertAsync("Erro", $"Falha ao ler recibo: {ex.Message}", "OK")!;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
