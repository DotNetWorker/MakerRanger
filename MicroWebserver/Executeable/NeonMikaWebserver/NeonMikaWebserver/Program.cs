using System.Threading;
using NeonMika.Webserver;
using NeonMika.Webserver.Responses;

namespace NeonMikaWebserverExecuteable
{
    public class Program
    {
        public static void Main()
        {
            Server WebServer = new Server(PinManagement.OnboardLED,80,false,"192.168.0.200","255.255.255.0","192.168.0.2","NETDUINOPLUS");
            WebServer.AddResponse(new XMLResponse("wave", new XMLResponseMethod(WebserverXMLMethods.Wave)));
        }
    }
}
