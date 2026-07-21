using GastoSmart.App.ViewModels;

namespace GastoSmart.App;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((DashboardViewModel)BindingContext).CarregarDadosAsync();
    }
}
