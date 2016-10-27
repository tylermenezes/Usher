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
        public string Provider { get { return "web"; } }
        public string Instance { get; protected set; }
        public string Uri
        {
            get
            {
                return string.Format("{0}://{1}", Provider, Instance);
            }
        }

        public event ManagerReadyHandler OnReady;
        public event ManagerStoppedHandler OnStop;
        public event ManagerErrorHandler OnError;

        private WebCommandSource commandSource;
        public IEnumerable<IDevice> Devices
        {
            get
            {
                return new IDevice[]{
                    commandSource
                };
            }
        }

        public Manager(string instance, Dictionary<string, string> config)
        {
            Instance = instance;
            commandSource = new WebCommandSource(this);
            listener.Prefixes.Add(String.Format("http://*:{0}/api/", instance));
        }

        public Task Start()
        {
            var semaphore = new SemaphoreSlim(0, 1);
            ThreadPool.QueueUserWorkItem((o) =>
            {
                listener.Start();
                semaphore.Release();
                OnReady(this);
                try
                {
                    while (listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            httpHandleRequest((HttpListenerContext)c);
                        }, listener.GetContext());
                    }
                }
                catch { OnError(this); }
            });
            return semaphore.WaitAsync();
        }

        public Task Stop()
        {
            var task = new Task(() => {
                listener.Stop();
                listener.Close();
            });
            task.Start();
            OnStop(this);
            return task;
        }

        protected readonly HttpListener listener = new HttpListener();
        protected void httpHandleRequest(HttpListenerContext ctx)
        {
            try
            {
                string rstr = httpResponse(ctx.Request);
                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                ctx.Response.ContentLength64 = buf.Length;
                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
            }
            catch (Exception e)
            {
                Utilities.Logger.Error(e);
                OnError(this);
            }
            finally
            {
                ctx.Response.OutputStream.Close();
            }
        }
        protected string httpResponse(HttpListenerRequest request)
        {
            var pathParts = request.Url.AbsolutePath.Split('/');
            var command = pathParts[2];
            var args = pathParts.Skip(3).ToArray();
            commandSource.dispatchCommand(command, args);
            return "ok";
        }
    }
}