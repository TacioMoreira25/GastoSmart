using GastoSmart.App;
using Microsoft.Extensions.DependencyInjection;

namespace Gastosmart.App;

public partial class App : Application
{
	private readonly LoginPage _loginPage;
	public App(LoginPage loginPage)
	{
		InitializeComponent();
	 _loginPage = loginPage;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(_loginPage);
	}
}