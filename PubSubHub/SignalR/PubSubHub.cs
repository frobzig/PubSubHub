using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PubSubHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PubSubHub.SignalR
{
    public static class GuidLink
    {
        private static readonly Dictionary<string, Guid> _link = new Dictionary<string, Guid>();

        public static Guid GetGuid(string id)
        {
            lock (_link)
            {
                Guid guid;

                if (!_link.TryGetValue(id, out guid))
                {
                    guid = Guid.NewGuid();
                    _link.Add(id, guid);
                }

                return guid;
            }
        }

        public static void Remove(string id)
        {
            lock (_link)
            {
                _link.Remove(id);
            }
        }
    }

    [HubName("pubSubHub")]
    public class PubSubHub : Hub
    {
        public override System.Threading.Tasks.Task OnConnected()
        {
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            GuidLink.Remove(this.Context.ConnectionId);
            this.Unsubscribe(null);

            return base.OnDisconnected(stopCalled);
        }
        
        public void PublishMessage(bool sendBack, string topic, dynamic content)
        {
            PubSubMessage message = new PubSubMessage();

            message.Content = content;
            message.TopicId = topic;

            SigMessageHub.Instance.PublishMessage(
                sendBack ? Guid.Empty : GuidLink.GetGuid(this.Context.ConnectionId),
                message);
        }

        public void Subscribe(string topic)
        {
            SigMessageHub.Instance.Subscribe(GuidLink.GetGuid(this.Context.ConnectionId), new Uri("signal://" + this.Context.ConnectionId), topic);
        }

        public void Unsubscribe(string topic)
        {
            SigMessageHub.Instance.Unsubscribe(GuidLink.GetGuid(this.Context.ConnectionId), topicId: topic);
        }
    }
}