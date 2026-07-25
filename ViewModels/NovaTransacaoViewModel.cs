using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GastoSmart.Models;
using GastoSmart.Services;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GastoSmart.ViewModels;

public partial class NovaTransacaoViewModel : ObservableObject
{
    private readonly ILocalDatabaseService _dbService;
    private readonly IUserSessionService _sessionService;
    private readonly IGroqVisionService _groqService;

    [ObservableProperty]
    private string descricao = string.Empty;

    [ObservableProperty]
    private decimal valor;

    [ObservableProperty]
    private string categoria = string.Empty;

    [ObservableProperty]
    private DateTime data = DateTime.Today;

    [ObservableProperty]
    private string caminhoImagem = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public NovaTransacaoViewModel(ILocalDatabaseService dbService, IUserSessionService sessionService, IGroqVisionService groqService)
    {
        _dbService = dbService;
        _sessionService = sessionService;
        _groqService = groqService;
    }

    [RelayCommand]
    private async Task SalvarTransacaoAsync()
    {
        if (string.IsNullOrWhiteSpace(Descricao))
        {
            await Shell.Current.DisplayAlertAsync("Erro", "A descrição é obrigatória.", "OK");
            return;
        }

        if (!_sessionService.IsLoggedIn) return;

        var nova = new Transacao
        {
            PerfilId = _sessionService.ActiveProfile!.Id,
            Descricao = Descricao,
            Valor = Valor,
            Categoria = Categoria,
            Data = Data,
            CaminhoImagem = CaminhoImagem
        };

        await _dbService.SaveTransacaoAsync(nova);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ScanReceiptAsync()
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    IsBusy = true;
                    
                    // Save local image
                    var localFilePath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);
                    using var sourceStream = await photo.OpenReadAsync();
                    using var localFileStream = File.OpenWrite(localFilePath);
                    await sourceStream.CopyToAsync(localFileStream);
                    
                    CaminhoImagem = localFilePath;

                    // Read base64
                    sourceStream.Position = 0;
                    using var memoryStream = new MemoryStream();
                    await sourceStream.CopyToAsync(memoryStream);
                    var base64 = Convert.ToBase64String(memoryStream.ToArray());

                    // Call Groq
                    var jsonResponse = await _groqService.AnalyzeReceiptAsync(base64);
                    
                    // Parse response
                    using var doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("descricao", out var descProp))
                        Descricao = descProp.GetString() ?? string.Empty;

                    if (root.TryGetProperty("valor", out var valorProp))
                        Valor = valorProp.GetDecimal();
                    
                    if (root.TryGetProperty("categoria", out var catProp))
                        Categoria = catProp.GetString() ?? string.Empty;
                        
                    if (root.TryGetProperty("data", out var dataProp))
                        if (DateTime.TryParse(dataProp.GetString(), out var d))
                            Data = d;
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Falha ao ler recibo: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
