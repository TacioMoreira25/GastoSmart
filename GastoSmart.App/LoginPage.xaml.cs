using Gastosmart.App;
using GastoSmart.App.Services;
using Microsoft.Maui.ApplicationModel;
using System;

namespace GastoSmart.App;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlertAsync("Atenção", "Preencha o e-mail e a senha.", "OK");
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
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current!.Windows[0].Page = new AppShell();
                });
            }
            else
            {
                await DisplayAlertAsync("Erro", "E-mail ou senha incorretos.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro de Conexão", "Não foi possível conectar ao servidor de autenticação.", "OK");
            System.Diagnostics.Debug.WriteLine($"[Login Error]: {ex.Message}");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            LoginButton.IsVisible = true;
        }
    }

    private void OnGoToRegisterClicked(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current!.Windows[0].Page = new RegisterPage(_authService);
        });
    }
}
