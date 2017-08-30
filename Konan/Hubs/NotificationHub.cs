using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Configuration;
using System.Data;
using Konan.Context;
using System.Web;

namespace Konan.Hubs
{
    [HubName("notificationHub")]
    public class NotificationHub : Hub
    {
        string connString = ConfigurationManager.ConnectionStrings["KonanDBContext"].ConnectionString;
        string id;

        public void RegisterMessage(string id_Msg)
        {
            if (id_Msg != null)
            {
                using(KonanDBContext _dc = new KonanDBContext())
                {
                    var msg = _dc.Messages.Where(m => m.Id == id_Msg).FirstOrDefault();
                    var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                    foreach (var conn in ChatHub.connectedUsers.Where(m => m.Id == msg.ToId))
                    {
                        context.Clients.Client(conn.ConnectionId).ReloadMsg(msg.FromId, 0);
                    }
                }
            }

        }

        public void RegisterLike(string Id_Like)
        {
            if(Id_Like != null)
            {
                using (KonanDBContext _dc = new KonanDBContext())
                {
                    var like = _dc.Likes.Where(m => m.Id == Id_Like).FirstOrDefault();
                    id = _dc.Posts.Where(p => p.Id == like.Id_P).FirstOrDefault().Id_A;
                    var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                    foreach (var conn in ChatHub.connectedUsers.Where(m => m.Id == id))
                    {
                        context.Clients.Client(conn.ConnectionId).UpdateNotification();
                    }
                }
            }        
        }

        public void RegisterComment(string Id_Comment)
        {

            if (Id_Comment != null)
            {
                using (KonanDBContext _dc = new KonanDBContext())
                {
                    var comm = _dc.Comments.Where(m => m.Id == Id_Comment).FirstOrDefault();
                    id = _dc.Posts.Where(p => p.Id == comm.Id_P).FirstOrDefault().Id_A;
                    var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                    foreach (var conn in ChatHub.connectedUsers.Where(m => m.Id == id))
                    {
                        context.Clients.Client(conn.ConnectionId).UpdateNotification();
                    }
                }
            }
        }

        public void RegisterShare(string Id_Share)
        {

            if (Id_Share != null)
            {
                using (KonanDBContext _dc = new KonanDBContext())
                {
                    var share = _dc.Posts.Where(m => m.Id == Id_Share).FirstOrDefault();
                    id = share.Id_A;
                    var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                    foreach (var conn in ChatHub.connectedUsers.Where(m => m.Id == id))
                    {
                        context.Clients.Client(conn.ConnectionId).UpdateNotification();
                    }
                }
            }
        }

        public void RegisterRequest(string Id_Request)
        {
            if (Id_Request != null)
            {
                using (KonanDBContext _dc = new KonanDBContext())
                {
                    var request = _dc.FriendRequests.Where(m => m.Id == Id_Request).FirstOrDefault();
                    id = request.Id_F;
                    var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                    foreach (var conn in ChatHub.connectedUsers.Where(m => m.Id == id))
                    {
                        context.Clients.Client(conn.ConnectionId).UpdateNotification();
                    }
                }
            }
        }

        public void RefreshComments(string Id_Post)
        {
            if(Id_Post != null)
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                context.Clients.All.RefreshComments(Id_Post);
            }
        }
    }
}