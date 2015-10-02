using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubSubHub.Models;

namespace PubSubHub
{
    public delegate void MessageReceivedDelegate(IPubSubMessage message);
}
