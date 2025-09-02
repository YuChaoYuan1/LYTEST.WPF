using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace LYTest.Mis.IMICP
{
    class HttpServer
    {
        private readonly HttpSelfHostServer server;
        //private readonly ServiceNotification serviceNotification;
        public HttpServer(string ip, int port)
        {
            var config = new HttpSelfHostConfiguration($"http://{ip}:{port}");
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{action}");
            server = new HttpSelfHostServer(config);
        }

        public Task StartHttpServer()
        {
            return server.OpenAsync();
        }
        public Task CloseHttpServer()
        {
            return server.CloseAsync();
        }
    }

}
