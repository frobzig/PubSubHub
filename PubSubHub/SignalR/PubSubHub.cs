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
    /// <summary>
    /// This class links a SignalR ConnectionId to a PubSubHub clientId
    /// </summary>
    internal static class GuidLink
    {
        internal class LinkInfo
        {
            public LinkInfo(string hubId = null)
            {
                this.HubId = hubId;
            }

            public Guid ClientId { get; set; } = Guid.NewGuid();
            public string HubId { get; set; }
        }

        private static readonly Dictionary<string, LinkInfo> _link = new Dictionary<string, LinkInfo>();

        internal static LinkInfo GetInfo(string connectionId)
        {
            lock (_link)
            {
                LinkInfo info;

                if (!_link.TryGetValue(connectionId, out info))
                {
                    info = new LinkInfo();
                    _link.Add(connectionId, info);
                }

                return info;
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
        private static SigMessageHub _messageHub;

        private static Func<string, SigMessageHub> _getMessageHub;
        public static Func<string, SigMessageHub> GetMessageHub
        {
            get
            {
                if (_getMessageHub == null)
                {
                    _messageHub = new SigMessageHub();
                    _getMessageHub = (_) => _messageHub;
                }

                return _getMessageHub;
            }
            set
            {
                _getMessageHub = value;
            }
        }

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
            string connectionId = this.Context.ConnectionId;

            GuidLink.LinkInfo info = GuidLink.GetInfo(connectionId);
            this.Unsubscribe(null, null, info.HubId);
            GuidLink.Remove(connectionId);

            return base.OnDisconnected(stopCalled);
        }

        public void PublishMessage(bool sendBack, string topic, string group, dynamic content, string hubId)
        {
            PubSubMessage message = new PubSubMessage();

            message.Content = content;
            message.TopicId = topic;
            message.GroupId = group;

            GetMessageHub(hubId).PublishMessage(
                sendBack ? Guid.Empty : GuidLink.GetInfo(this.Context.ConnectionId).ClientId,
                message);
        }

        public void Subscribe(string topic, string group, string hubId)
        {
            GetMessageHub(hubId).Subscribe(
                GuidLink.GetInfo(this.Context.ConnectionId).ClientId,
                new Uri("signal://" + this.Context.ConnectionId),
                topic, group);
        }

        public void Unsubscribe(string topic, string group, string hubId)
        {
            GetMessageHub(hubId).Unsubscribe(
                GuidLink.GetInfo(this.Context.ConnectionId).ClientId,
                topicId: topic,
                groupId: group);
        }
    }
}