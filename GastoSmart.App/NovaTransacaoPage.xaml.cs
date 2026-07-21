using GastoSmart.App.ViewModels;

namespace GastoSmart.App;

public partial class NovaTransacaoPage : ContentPage
{
    public NovaTransacaoPage(NovaTransacaoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
