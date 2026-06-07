using Gastosmart.App;
using GastoSmart.App.Services;

namespace GastoSmart.App;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Atenção", "Preencha o e-mail e a senha.", "OK");
            return;
        }

        LoginButton.IsVisible = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            var success = await _authService.LoginAsync(email, password);

            if (success)
            {
                Application.Current!.Windows[0].Page = new AppShell();
            }
            else
            {
                await DisplayAlert("Erro", "E-mail ou senha incorretos.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Conexão", "Não foi possível conectar ao servidor de autenticação.", "OK");
            System.Diagnostics.Debug.WriteLine($"[Login Error]: {ex.Message}");
        }
        finally
        {
            // Restaura o botão
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            LoginButton.IsVisible = true;
        }
    }
}