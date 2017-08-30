using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using Konan.Models;
using Konan.Context;

namespace Konan.Hubs
{
    [HubName("chatHub")]
    public class ChatHub : Hub
    {
        public static HashSet<Account> connectedUsers = new HashSet<Account>();

        public override Task OnConnected()
        {
            using (KonanDBContext _dc = new KonanDBContext())
            {
                string id = HttpContext.Current.Request.Cookies["AuthID"].Value;
                var acc = _dc.Accounts.Where(m => m.Id == id).FirstOrDefault();

                acc.ConnectionId = Context.ConnectionId;
                connectedUsers.Add(acc);
            }

            Clients.All.UpdateCount(connectedUsers.GroupBy(m => m.Email).Select(g => g.First()).ToList().Count);
            Clients.All.UpdateChat(connectedUsers.GroupBy(m => m.Email).Select(g => g.First()).ToList());

            return base.OnConnected();
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            connectedUsers.RemoveWhere(m => m.ConnectionId == Context.ConnectionId);

            Clients.All.UpdateCount(connectedUsers.GroupBy(m => m.Email).Select(g => g.First()).ToList().Count);
            Clients.All.UpdateChat(connectedUsers.GroupBy(m => m.Email).Select(g => g.First()).ToList());

            return base.OnDisconnected(stopCalled);
        }
    }
}