using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TgpBudget.Startup))]
namespace TgpBudget
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
