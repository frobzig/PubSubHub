using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PubSubHub.Models;

namespace PubSubHub.Relay
{
    public class HttpPeerRelay : PeerRelay
    {
        public HttpPeerRelay(Peer peer, IPeerProvider peerProvider, IMessageHub hub)
            : base(peer, peerProvider, hub)
        {
        }

        async protected override void SendMessage(Peer peer, IPubSubMessage message)
        {
            await HttpPusher.PublishMessage(peer.Address, message);
        }
    }
}
