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
            return results.FirstOrDefault();
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

            // Testa conectividade com a API primeiro
            System.Diagnostics.Debug.WriteLine("[MainPage] Testing API connectivity...");
            
            // Envia para a API local
            _currentTransaction = await _apiService.ScanReceiptAsync(photo);

            if (_currentTransaction != null)
            {
                // Preenche os campos originais
                TitleLabel.Text = string.IsNullOrWhiteSpace(_currentTransaction.Title) ? "Nenhum título detetado" : _currentTransaction.Title;
                AmountLabel.Text = _currentTransaction.Amount.ToString("C"); // Formata como moeda local (ex: R$ 15,00 ou 15,00 €)
                DateLabel.Text = _currentTransaction.Date.ToString("dd/MM/yyyy");
                
                // Preenche os novos campos expandidos
                CategoryLabel.Text = _currentTransaction.CategoryId != Guid.Empty 
                    ? _currentTransaction.CategoryId.ToString() 
                    : "Sem categoria associada";
                
                // Torna o quadro visível!
                ResultFrame.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainPage] Exception: {ex}");
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
        if (_currentTransaction == null) return;

        try
        {
            // Mostramos a bolinha a rodar para o utilizador saber que estamos a gravar
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            
            // Chama a API para salvar
            var success = await _apiService.SaveTransactionAsync(_currentTransaction);

            if (success)
            {
                await DisplayAlertAsync("Sucesso!", "O seu gasto foi salvo no banco de dados.", "OK");
                
                // Limpamos o ecrã para estarmos prontos para a próxima foto!
                ResultFrame.IsVisible = false;
                ReceiptImage.Source = null;
                _currentTransaction = null;
            }
            else
            {
                await DisplayAlertAsync("Ops", "Não foi possível salvar o gasto. Verifique a consola.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro Técnico", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}