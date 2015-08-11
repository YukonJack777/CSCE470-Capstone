using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProjectPortal.Startup))]
namespace ProjectPortal
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
