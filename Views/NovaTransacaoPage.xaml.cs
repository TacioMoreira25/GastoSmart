using GastoSmart.ViewModels;

namespace GastoSmart.Views;

public partial class NovaTransacaoPage : ContentPage
{
    public NovaTransacaoPage(NovaTransacaoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
