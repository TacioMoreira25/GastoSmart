using GastoSmart.App.Services;
using Microsoft.Maui.ApplicationModel;
using System.Text.RegularExpressions;
using System;

namespace GastoSmart.App;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _authService;

    public RegisterPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? string.Empty;
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;
        var confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || 
            string.IsNullOrEmpty(confirmPassword))
        {
            await DisplayAlertAsync("Atenção", "Por favor, preencha todos os campos.", "OK");
            return;
        }

        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(email))
        {
            await DisplayAlertAsync("Atenção", "Por favor, insira um endereço de e-mail válido.", "OK");
            return;
        }

        if (password.Length < 8)
        {
            await DisplayAlertAsync("Atenção", "A senha deve ter no mínimo 8 caracteres.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlertAsync("Erro", "As senhas não coincidem.", "OK");
            return;
        }

        RegisterButton.IsVisible = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        var result = await _authService.RegisterAsync(name, email, password);

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
        RegisterButton.IsVisible = true;

        if (result.IsSuccess)
        {
            await DisplayAlertAsync("Sucesso", "Conta criada com sucesso! Faça login.", "OK");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current!.Windows[0].Page = new LoginPage(_authService);
            });
        }
        else
        {
            await DisplayAlertAsync("Erro", result.ErrorMessage, "OK");
        }
    }

    private void OnBackToLoginClicked(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Application.Current!.Windows[0].Page = new LoginPage(_authService);
        });
    }
}
