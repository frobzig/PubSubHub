using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubSubHub.Models;

namespace PubSubHub.Relay
{
    public abstract class PeerRelay : IRelay
    {
        private readonly IPeerProvider _peerProvider;
        private readonly IMessageHub _hub;

        public Peer Peer { get; private set; }

        public PeerRelay(Peer peer, IPeerProvider peerProvider, IMessageHub hub)
        {
            this.Peer = peer;

            this._peerProvider = peerProvider;
            this._hub = hub;
        }

        public void Propagate(IPubSubMessage message)
        {
            IEnumerable<Peer> peers = this._peerProvider.GetPeers().Where(p => p != this.Peer);
            List<Peer> targets = new List<Peer>();

            foreach (Peer peer in peers)
            {
                if (!message.Peers.Contains(peer))
                {
                    message.Peers.Add(peer);
                    targets.Add(peer);
                }
            }

            foreach (Peer target in targets)
            {
                this.SendMessage(target, message);
            }
        }

        protected virtual void SendMessage(Peer peer, IPubSubMessage message)
        {
        }
    }
}
