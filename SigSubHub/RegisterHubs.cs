using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System;
using System.Web;
using System.Web.Routing;

[assembly: OwinStartup(typeof(SigSubHub.Startup))]

namespace SigSubHub
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AppDomain.CurrentDomain.Load(typeof(PubSubHub.SignalR.SigMessageHub).Assembly.FullName);
            app.MapSignalR();
        }
    }
}