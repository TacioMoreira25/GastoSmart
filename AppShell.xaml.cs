using GastoSmart.Views;

namespace GastoSmart;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("DashboardPage", typeof(DashboardPage));
		Routing.RegisterRoute("NovaTransacaoPage", typeof(NovaTransacaoPage));
	}
}
