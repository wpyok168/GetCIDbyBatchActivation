using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GetCIDbyBatchActivation
{
    public class XmlRequest
    {
        public static void MSXmlRequest(int typeid, string iid, string pid)
        {
            XDocument xd = CreateXml(typeid, iid, pid);
            XDocument resulttxd = CreateWebRequest(xd);
            XNamespace xn1 = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace xn2 = "http://www.microsoft.com/BatchActivationService";
            XElement xe = resulttxd.Descendants(xn2+ "ResponseXml").FirstOrDefault();
            XDocument xd1 = XDocument.Parse(xe.Value);

            XNamespace xn = "http://www.microsoft.com/DRM/SL/BatchActivationResponse/1.0";
            XElement xe1 = xd1.Descendants(xn+ "CID").FirstOrDefault();
            XElement errr = xd1.Descendants(xn + "ErrorCode").FirstOrDefault();

            XElement xe2 = xd1.Root.Element(xn+ "Responses").Element(xn + "Response").Element(xn + "CID");

            XmlNamespaceManager xnm = new XmlNamespaceManager(new NameTable());
            xnm.AddNamespace("ar", "http://www.microsoft.com/DRM/SL/BatchActivationResponse/1.0");
            XElement xe3 = xd1.XPathSelectElement("//ar:CID", xnm);
            XElement err = xd1.XPathSelectElement("//ar:ErrorCode", xnm);
            //string errstr = err.Value;
            //错误代码 0x7F:Exceeded; 0x67:; 0xD5:Blocked; 0x68:Invalidkey; 0x86:InvalidType; 0x90:IIDError; 0x71:NeverObtained;
        }
        private static XDocument CreateXml(int typeid, string iid, string pid)
        {
            XNamespace xn = "http://www.microsoft.com/DRM/SL/BatchActivationRequest/1.0";
            XElement xelement = new XElement(xn + "ActivationRequest", new object[]
            {
                new XElement(xn + "VersionNumber","2.0"),
                new XElement(xn + "RequestType",typeid),
                new XElement(xn + "Requests",new object[]
                {
                    new XElement(xn + "Request",new object[]
                    {
                        new XElement(xn + "PID",pid),
                        new XElement(xn + "IID",iid),
                    })
                }),
            });
            byte[] bytes = Encoding.Unicode.GetBytes(xelement.ToString());
            string RequestXml = Convert.ToBase64String(bytes); //RequestXml

            HMACSHA256 hmacsha256 = new HMACSHA256
            {
                Key = BPrivateKey
            };
            string Digest = Convert.ToBase64String(hmacsha256.ComputeHash(bytes));

            XNamespace xn1 = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace xn2 = "http://www.microsoft.com/BatchActivationService";
            XDocument xDocument = new XDocument(new XDeclaration("1.0", "UTF-8", "no"),new object[]
            {
                new XElement(xn1+"Envelope",new object[]
                {
                    new XAttribute(XNamespace.Xmlns + "soap","http://schemas.xmlsoap.org/soap/envelope/"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    
                    new XElement(xn1 + "Body", new XElement(xn2 + "BatchActivate",new XElement(xn2 + "request",new object[]
                    {
                        new XElement(xn2 + "Digest", Digest),
                        new XElement(xn2 + "RequestXml", RequestXml)
                    })))
                })
            });;
            return xDocument;
        }

        private static readonly byte[] BPrivateKey = new byte[]
        {
            0xfe, 0x31, 0x98, 0x75, 0xfb, 0x48, 0x84, 0x86, 0x9c, 0xf3, 0xf1, 0xce, 0x99, 0xa8, 0x90, 0x64,
            0xab, 0x57, 0x1f, 0xca, 0x47, 0x04, 0x50, 0x58, 0x30, 0x24, 0xe2, 0x14, 0x62, 0x87, 0x79, 0xa0,
        };

        private static XDocument CreateWebRequest(XDocument soapRequest)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri("https://activation.sls.microsoft.com/BatchActivation/BatchActivation.asmx"));
            httpWebRequest.Accept = "text/xml";
            httpWebRequest.ContentType = "text/xml; charset=\"utf-8\"";
            httpWebRequest.Headers.Add("SOAPAction", "http://www.microsoft.com/BatchActivationService/BatchActivate");
            httpWebRequest.Host = "activation.sls.microsoft.com";
            httpWebRequest.Method = "POST";
            try
            {
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    soapRequest.Save(requestStream);
                }
            }
            catch
            {
                throw;
            }
            HttpWebResponse res = null;
            string outhtml = string.Empty;
            XDocument xd1=null;
            try
            {
                using (res = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (res.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        outhtml = new StreamReader(new GZipStream(res.GetResponseStream(), CompressionMode.Decompress)).ReadToEnd();
                    }
                    else if (res.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        outhtml = new StreamReader(new DeflateStream(res.GetResponseStream(), CompressionMode.Decompress)).ReadToEnd();
                    }
                    else
                    {
                        outhtml = new StreamReader(res.GetResponseStream()).ReadToEnd();
                    }

                    if (outhtml != string.Empty && res.ContentType.Contains("text/xml"))
                    {
                         xd1 = XDocument.Parse(outhtml);
                    }
                }
            }
            catch (Exception)
            {

            }


            return xd1;
        }
    }
}
