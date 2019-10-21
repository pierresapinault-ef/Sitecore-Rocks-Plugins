namespace Sitecore.Rocks.Server.Plugins.Requests
{
    using System.IO;
    using System.Xml;

    public class HandleRequest
    {
        public string Execute()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("result");

            // Write result here.

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}