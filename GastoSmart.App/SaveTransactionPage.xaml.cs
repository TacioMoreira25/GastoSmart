namespace GastoSmart.App;

[QueryProperty(nameof(TransactionData), "TransactionData")]
public partial class SaveTransactionPage 
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

    public SaveTransactionPage(object transactionData)
    {
        _transactionData = transactionData;
        InitializeComponent();
        new Command(async void () => await Shell.Current.GoToAsync(".."));
        BindingContext = this;
    }
}
