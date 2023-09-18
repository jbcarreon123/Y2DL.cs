using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Xml;

namespace PubSubCallbackHandler.Controllers
{
    [Route("callback")]
    public class CallbackController : ControllerBase
    {
        [HttpPost]
        public IActionResult ReceiveCallback()
        {
            // Handle the incoming PubSubHubbub callback here.
            
            // For example, you can parse the incoming XML data.
            using (var reader = new StreamReader(Request.Body))
            {
                var xmlData = reader.ReadToEnd();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);
                
                // Process the XML data as needed.
            }

            // Respond with a 200 OK status to acknowledge the callback.
            return Ok();
        }
    }
}