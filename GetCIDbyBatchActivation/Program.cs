using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Data.SqlTypes;
using System.Xml.XPath;

namespace GetCIDbyBatchActivation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            testc();
            //string iid = "209200177437440198194193547128466754585074098382223107026650642";
            string iid = "330512311875727788090139496334835149954645445802270599047875924";
            XmlRequest.MSXmlRequest(1, iid, "00000-04249-038-820384-03-2052-9200.0000-0902023");

            ActivationHelper.CallWebService(1, iid, "00000-04249-038-820384-03-2052-9200.0000-0902023");
            Console.ReadKey();
        }

        static void testc()
        {
            string xmlString = "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><soap:Body><BatchActivateResponse xmlns=\"http://www.microsoft.com/BatchActivationService\"><BatchActivateResult><ResponseXml>&lt;?xml version=\"1.0\" encoding=\"utf-16\"?&gt;\r\n&lt;ActivationResponse xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://www.microsoft.com/DRM/SL/BatchActivationResponse/1.0\"&gt;\r\n  &lt;VersionNumber&gt;1.0&lt;/VersionNumber&gt;\r\n  &lt;ResponseType&gt;1&lt;/ResponseType&gt;\r\n  &lt;BatchId&gt;{976cbe56-21e5-4e13-bdc3-4e72d7ab48fd}&lt;/BatchId&gt;\r\n  &lt;Responses&gt;\r\n    &lt;Response&gt;\r\n      &lt;PID&gt;00000-04249-038-820384-03-2052-9200.0000-0902023&lt;/PID&gt;\r\n      &lt;ActivationRemaining&gt;-1&lt;/ActivationRemaining&gt;\r\n      &lt;IID&gt;330512311875727788090139496334835149954645445802270599047875924&lt;/IID&gt;\r\n      &lt;CID&gt;429292031401597794898455690612212251894655189294&lt;/CID&gt;\r\n    &lt;/Response&gt;\r\n  &lt;/Responses&gt;\r\n&lt;/ActivationResponse&gt;</ResponseXml></BatchActivateResult></BatchActivateResponse></soap:Body></soap:Envelope>";
            XDocument xmlDocument = XDocument.Parse(xmlString);

            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            xnm.AddNamespace("msNs", "http://www.microsoft.com/BatchActivationService");
            xnm.AddNamespace("drmNs", "http://www.microsoft.com/DRM/SL/BatchActivationResponse/1.0");
            XElement ResponseXml = xmlDocument.XPathSelectElement("//soap:Envelope/soap:Body/msNs:BatchActivateResponse/msNs:BatchActivateResult/msNs:ResponseXml", xnm);

            XDocument xmlDocument1 = XDocument.Parse(ResponseXml.Value);

            XmlNamespaceManager xnm1 = new XmlNamespaceManager(new NameTable());
            xnm1.AddNamespace("drmNs", "http://www.microsoft.com/DRM/SL/BatchActivationResponse/1.0");
            XElement CID = xmlDocument1.XPathSelectElement("//drmNs:CID", xnm1);

        }
    }
}
