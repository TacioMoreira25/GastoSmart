using GastoSmart.ViewModels;

namespace GastoSmart.Views;

public partial class PerfilSelectionPage : ContentPage
{
    private readonly PerfilSelectionViewModel _viewModel;

    public PerfilSelectionPage(PerfilSelectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadPerfisAsync();
    }
}
