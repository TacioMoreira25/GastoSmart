namespace GastoSmart.App;

[QueryProperty(nameof(TransactionData), "TransactionData")]
public partial class SaveTransactionPage : ContentPage
{
    private object _transactionData;
    public object TransactionData
    {
        get => _transactionData;
        set
        {
            _transactionData = value;
            OnPropertyChanged();
        }
    }

    public Command GoBackCommand { get; }

    public SaveTransactionPage()
    {
        InitializeComponent();
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        BindingContext = this;
    }
}
