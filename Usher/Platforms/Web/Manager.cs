using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Usher.Platforms.Generic;
using Usher.Platforms.Generic.Devices;
using Usher.Platforms.Web.Devices;
namespace Usher.Platforms.Web
{
    [ManagerAttribute("web")]
    public class Manager : IManager
    {
        public string Provider => "web";
        public string Instance { get; protected set; }
        public string Uri => $"{Provider}://{Instance}";

        public event ManagerReadyHandler OnReady;
        public event ManagerStoppedHandler OnStop;
        public event ManagerErrorHandler OnError;

        private readonly WebCommandSource _commandSource;
        public IEnumerable<IDevice> Devices => new IDevice[]{ _commandSource };

        public Manager(string instance, Dictionary<string, string> config)
        {
            Instance = instance;
            _commandSource = new WebCommandSource(this);
            Listener.Prefixes.Add($"http://*:{instance}/api/");
        }

        public Task Start()
        {
            var semaphore = new SemaphoreSlim(0, 1);
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Listener.Start();
                semaphore.Release();
                OnReady?.Invoke(this);
                try
                {
                    while (Listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            HttpHandleRequest((HttpListenerContext)c);
                        }, Listener.GetContext());
                    }
                }
                catch { OnError(this); }
            });
            return semaphore.WaitAsync();
        }

        public Task Stop()
        {
            var task = new Task(() => {
                Listener.Stop();
                Listener.Close();
            });
            task.Start();
            OnStop?.Invoke(this);
            return task;
        }

        protected readonly HttpListener Listener = new HttpListener();
        protected void HttpHandleRequest(HttpListenerContext ctx)
        {
            try
            {
                var rstr = HttpResponse(ctx.Request);
                var buf = Encoding.UTF8.GetBytes(rstr);
                ctx.Response.ContentLength64 = buf.Length;
                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
            }
            catch (Exception e)
            {
                Utilities.Logger.Error(e);
                OnError?.Invoke(this);
            }
            finally
            {
                ctx.Response.OutputStream.Close();
            }
        }
        protected string HttpResponse(HttpListenerRequest request)
        {
            var pathParts = request.Url.AbsolutePath.Split('/');
            var command = pathParts[2];
            var args = pathParts.Skip(3).ToArray();
            _commandSource.DispatchCommand(command, args);
            return "ok";
        }
    }
}