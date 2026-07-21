namespace Gastosmart.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("NovaTransacaoPage", typeof(GastoSmart.App.NovaTransacaoPage));
	}
}
