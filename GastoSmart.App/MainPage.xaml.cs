using Gastosmart.App.DTOs;
using GastoSmart.App.Services;

namespace GastoSmart.App;

public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;
    private TransactionRequestDTO? _currentTransaction;

    public MainPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        await ProcessPhotoAsync(options => MediaPicker.Default.CapturePhotoAsync(options));
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
    {
        await ProcessPhotoAsync(async options => 
        {
            var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = options.Title });
            return results?.FirstOrDefault();
        });
    }

    private async Task ProcessPhotoAsync(Func<MediaPickerOptions, Task<FileResult?>> photoFunc)
    {
        try
        {
            var photo = await photoFunc(new MediaPickerOptions { Title = "Selecione o recibo" });
            if (photo == null) return;

            // Mostra a foto que você acabou de tirar na tela
            var stream = await photo.OpenReadAsync();
            ReceiptImage.Source = ImageSource.FromStream(() => stream);

            // Mostra a bolinha girando e esconde o resultado antigo
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            ResultFrame.IsVisible = false;

            // Envia para a API local
            _currentTransaction = await _apiService.ScanReceiptAsync(photo);

            if (_currentTransaction != null)
            {
                // Preenche a tela com os dados que vieram do Groq
                TitleLabel.Text = _currentTransaction.Title;
                AmountLabel.Text = _currentTransaction.Amount.ToString("C"); // Formata como moeda
                DateLabel.Text = _currentTransaction.Date.ToString("dd/MM/yyyy");
                
                ResultFrame.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            // Se falhar a comunicação, vai mostrar EXATAMENTE qual foi o erro aqui
            await DisplayAlertAsync("Erro Técnico", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnSaveTransactionClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync("Próximo Passo", "Aqui faremos a chamada POST para salvar no banco de dados!", "OK");
    }
}