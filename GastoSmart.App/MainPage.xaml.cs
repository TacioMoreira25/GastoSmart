using Gastosmart.App.Services;

namespace Gastosmart.App;

public partial class MainPage : ContentPage
{
	private readonly ApiService _apiService;

	public MainPage(ApiService apiService)
	{
		InitializeComponent();
		_apiService = apiService;
	}

	private async void OnTakePhotoClicked(object sender, EventArgs e)
	{
		try
		{
			if (MediaPicker.Default.IsCaptureSupported)
			{
				var photo = await MediaPicker.Default.CapturePhotoAsync();
				await ProcessPhotoAsync(photo);
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Falha ao abrir a câmera: {ex.Message}", "OK");
		}
	}

	private async void OnPickPhotoClicked(object sender, EventArgs e)
	{
		try
		{
			var photo = await MediaPicker.Default.PickPhotoAsync();
			await ProcessPhotoAsync(photo);
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Falha ao selecionar foto: {ex.Message}", "OK");
		}
	}

	private async Task ProcessPhotoAsync(FileResult? photo)
	{
		if (photo == null) return;

		LoadingIndicator.IsRunning = true;
		LoadingIndicator.IsVisible = true;
		TitleLabel.Text = string.Empty;
		AmountLabel.Text = string.Empty;
		DateLabel.Text = string.Empty;

		try
		{
			var result = await _apiService.ScanReceiptAsync(photo);

			if (result != null)
			{
				TitleLabel.Text = $"Título: {result.Title}";
				AmountLabel.Text = $"Valor: {result.Amount:C2}";
				DateLabel.Text = $"Data: {result.Date:dd/MM/yyyy}";
			}
			else
			{
				await DisplayAlert("Erro", "Não foi possível processar o recibo.", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Erro", $"Falha de comunicação com a API: {ex.Message}", "OK");
		}
		finally
		{
			LoadingIndicator.IsRunning = false;
			LoadingIndicator.IsVisible = false;
		}
	}
}
