using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PubSubHub.SelfHost;

namespace PubSubHub.ConsoleHost
{
    public class ConsoleMessageHub : MemoryMessageHub
    {
        public string Name { get; set; }

        public ConsoleMessageHub(string name)
        {
            this.Name = name;
        }

        protected override void OnMessageAdded(Models.IPubSubMessage message)
        {
            Console.WriteLine(String.Format("messaged added to {0}", this.Name));
        }        
    }

    class Program
    {
        static void Main(string[] args)
        {
            IMessageHub hub1 = new ConsoleMessageHub("hub1");
            IMessageHub hub2 = new ConsoleMessageHub("hub2");
            IMessageHub hub3 = new ConsoleMessageHub("hub3");

            HttpPubSubListener listener1 = new HttpPubSubListener(hub1);
            HttpPubSubListener listener2 = new HttpPubSubListener(hub2);
            HttpPubSubListener listener3 = new HttpPubSubListener(hub3);

            listener1.Start("http://+:10500/hub1/");
            listener2.Start("http://+:10500/hub2/");
            listener3.Start("http://+:10500/hub3/");

            Console.WriteLine("Listening...");

            Console.ReadKey();
        }
    }
}
