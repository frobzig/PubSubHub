using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System.Web;
using System.Web.Routing;

[assembly: OwinStartup(typeof(SigSubHub.Startup))]

namespace SigSubHub
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}