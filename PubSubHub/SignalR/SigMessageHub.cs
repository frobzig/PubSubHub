using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PubSubHub;
using PubSubHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PubSubHub.SignalR
{
    public class SigMessageHub : MemoryMessageHub
    {
        private static readonly SigMessageHub _instance = new SigMessageHub();

        public static SigMessageHub Instance
        {
            get
            {
                return _instance;
            }
        }

        public SigMessageHub()
        {
            this.MessagePublished += SigMessageHub_MessagePublished;
        }

        ~SigMessageHub()
        {
            this.MessagePublished -= SigMessageHub_MessagePublished;
        }

        private void SigMessageHub_MessagePublished(ISubscriptionInfo cbInfo, IPubSubMessage message)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<PubSubHub>();

            if (cbInfo.Uri.Scheme == "signal")
            {
                context.Clients.Client(cbInfo.Uri.Host).receiveMessage(message);
            }
        }
    }
}