using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Usher.Plugins.TylerMenezes.Unifi
{
    public class UnifiApiWorker
    {
        public readonly Uri UnifiBase;
        public readonly string Username;
        public readonly string Password;

        protected HttpClient Client = null;

        public UnifiApiWorker(Uri unifiBase, string username, string password)
        {
            UnifiBase = unifiBase;
            Username = username;
            Password = password;
        }

        protected async Task Login()
        {
            // Create new HTTP Clientt
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler {CookieContainer = cookies};
            Client = new HttpClient {BaseAddress = UnifiBase};

            // TODO: Improve this
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            // Login
            var postJson = new JObject();
            postJson["username"] = Username;
            postJson["password"] = Password;
            postJson["strict"] = true;

            var postString = new StringContent(
                postJson.ToString(),
                Encoding.UTF8,
                "application/json");

            // Do the login
            var result = (await Client.PostAsync("/api/login", postString))
                            .Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(result);

            // Check the result
            if ((string) json["meta"]["rc"] != "ok")
            {
                throw new ArgumentException("Unvalid username or password");
            }
        }

        protected async Task<JObject> DoApiRequest(string endpoint)
        {
            if (Client == null) await Login();

            var result = (await Client.GetAsync(endpoint))
                            .Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(result);

            if ((string) json["meta"]["msg"] == "api.err.LoginRequired")
            {
                await Login();
                return await DoApiRequest(endpoint);
            }
            else if ((string) json["meta"]["rc"] == "error")
            {
                throw new Exception($"Unifi returned error: {result}");
            }

            return json;
        }

        public async Task<IEnumerable<string>> GetClients()
        {
            // TODO: This should support more than just the default
            var result = await DoApiRequest("/api/s/default/stat/sta");

            return ((IEnumerable<JToken>) result["data"])
                .Select(p => (string) p["mac"]);
        }
    }
}
