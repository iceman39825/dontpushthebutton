using System;
using Microsoft.ServiceBus.Messaging;
using System.Web.Mvc;

namespace FrontendWebRole.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Push()
        {
            var connectionString = "Endpoint=sb://dontpushthebutton.servicebus.windows.net/;SharedAccessKeyName=FrontEndSend;SharedAccessKey=mIS1280TbuAmpCPRab4VDxjxX5SMgbzCo/O4mP/Ww2w=";
            var queueName = "dontpushthebutton";

            var client = QueueClient.CreateFromConnectionString(connectionString, queueName);
            var message = new BrokeredMessage(DateTime.UtcNow);
            client.Send(message);

            return View();
        }
    }
}