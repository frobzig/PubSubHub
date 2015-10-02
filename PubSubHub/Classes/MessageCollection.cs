using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PubSubHub.Models;

namespace PubSubHub
{
    public class MessageCollection : Collection<IPubSubMessage>
    {
        public MessageCollection()
        {
        }

        public MessageCollection(IEnumerable<IPubSubMessage> messages)
        {
            foreach (IPubSubMessage message in messages)
            {
                this.Add(message);
            }
        }

        public DateTime SinceDate { get; set; }
    }
}
