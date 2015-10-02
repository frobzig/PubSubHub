using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubSubHub.Models;

namespace PubSubHub.SelfHost
{
    public class HttpPubSubListener
    {
        private HttpListener _listener;
        private readonly IMessageHub _hub;

        public HttpPubSubListener(IMessageHub hub)
        {
            this._hub = hub;
        }

        private void OnGetContext(IAsyncResult result)
        {
            lock (this)
            {
                if (this._listener == null)
                    return;

                this._listener.BeginGetContext(this.OnGetContext, this._listener);

                HttpListenerContext ctx = this._listener.EndGetContext(result);
                StreamReader reader = new StreamReader(ctx.Request.InputStream);

                try
                {
                    string json = reader.ReadToEnd();

                    IPubSubMessage message = JsonConvert.DeserializeObject<PubSubMessage>(json);

                    this._hub.PublishMessage(message.ClientId, message);

                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentLength64 = 0;
                }
                finally
                {
                    reader.Close();
                    ctx.Response.Close();
                }
            }
        }

        public void Start(string prefix)
        {
            lock (this)
            {
                if (this._listener != null)
                    return;

                this._listener = new HttpListener();
                this._listener.Prefixes.Add(prefix);

                this._listener.Start();

                this._listener.BeginGetContext(this.OnGetContext, this._listener);
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (this._listener == null)
                    return;

                this._listener.Stop();
                this._listener = null;
            }
        }
    }
}
