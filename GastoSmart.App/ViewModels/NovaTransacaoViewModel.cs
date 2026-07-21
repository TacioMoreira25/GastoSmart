using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace GastoSmart.App.ViewModels;

public partial class NovaTransacaoViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Descricao { get; set; } = string.Empty;

    [ObservableProperty]
    public partial decimal Valor { get; set; }

    [ObservableProperty]
    public partial DateTime Data { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string Tipo { get; set; } = "Despesa";

    private readonly GastoSmart.App.Services.ApiService _apiService;

    public NovaTransacaoViewModel(GastoSmart.App.Services.ApiService apiService)
    {
        _apiService = apiService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotScanning))]
    public partial bool IsScanning { get; set; }

    public bool IsNotScanning => !IsScanning;

    [RelayCommand]
    public async Task EscanearReciboAsync()
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var action = await Shell.Current.DisplayActionSheet("Escanear Recibo", "Cancelar", null, "Tirar Foto", "Escolher da Galeria");
                
                FileResult? photo = null;

                if (action == "Tirar Foto")
                {
                    photo = await MediaPicker.Default.CapturePhotoAsync();
                }
                else if (action == "Escolher da Galeria")
                {
                    photo = await MediaPicker.Default.PickPhotoAsync();
                }

                if (photo != null)
                {
                    IsScanning = true;
                    var transactionData = await _apiService.ScanReceiptAsync(photo);
                    
                    if (transactionData != null)
                    {
                        Descricao = transactionData.Title;
                        Valor = transactionData.Amount;
                        if (transactionData.Date != default)
                        {
                            Data = transactionData.Date;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Falha ao ler recibo: {ex.Message}", "OK");
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    public async Task SalvarAsync()
    {
        // Por enquanto apenas retorna
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task CancelarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
