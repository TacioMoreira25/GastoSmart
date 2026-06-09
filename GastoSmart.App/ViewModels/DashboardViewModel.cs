using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gastosmart.App.DTOs;
using GastoSmart.App.Services;
// using Microcharts;
// using SkiaSharp;

namespace GastoSmart.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    public partial decimal TotalBalance { get; set; }

    [ObservableProperty]
    public partial decimal MonthlyExpenses { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    // [ObservableProperty]
    // public partial Chart? ExpensesChart { get; set; }
    public ObservableCollection<TransactionResponseDTO> RecentTransactions { get; } = new();

    public DashboardViewModel(ApiService apiService, decimal monthlyExpenses)
    {
        _apiService = apiService;
        MonthlyExpenses = monthlyExpenses;
    }

    [RelayCommand]
    public async Task LoadDashboardDataAsync()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            var summary = await _apiService.GetDashboardSummaryAsync();
            if (summary != null)
            {
                TotalBalance = summary.TotalBalance;
                MonthlyExpenses = summary.MonthlyExpenses;

                RecentTransactions.Clear();
                foreach (var tx in summary.RecentTransactions)
                {
                    RecentTransactions.Add(tx);
                }

                // UpdateChart(summary.CategorySummaries);
            }
        }
        catch (UnauthorizedAccessException)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CRASH EVITADO NO DASHBOARD]: {ex.Message}\n{ex.StackTrace}");
        
            MainThread.BeginInvokeOnMainThread(async void () =>
            {
                await Application.Current?.Windows[0].Page?.DisplayAlertAsync("Erro ao Carregar", "Ocorreu um erro ao montar o painel.", "OK")!;
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    /*
    private void UpdateChart(List<CategorySummaryDTO> categories)
    {
        var entries = new List<ChartEntry>();
        var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#F9D56E", "#FF8C42", "#8B8BC3" };
        int colorIndex = 0;

        foreach (var category in categories)
        {
            entries.Add(new ChartEntry((float)category.TotalAmount)
            {
                Label = category.CategoryName,
                ValueLabel = category.TotalAmount.ToString("C"),
                Color = SKColor.Parse(colors[colorIndex % colors.Length])
            });
            colorIndex++;
        }

        ExpensesChart = new DonutChart
        {
            Entries = entries,
            LabelTextSize = 30,
            BackgroundColor = SKColors.Transparent,
            HoleRadius = 0.5f,
            LabelMode = LabelMode.RightOnly
        };
    }
    */

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
