using Microsoft.AspNet.SignalR;
using Owin;

namespace Konan.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR("/signalr", new HubConfiguration());
        }
    }
}