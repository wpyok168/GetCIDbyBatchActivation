using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace AutoCall
{
	// Token: 0x02000002 RID: 2
	public class ActivationHelper
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
		public static string CallWebService(int requestType, string installationId, string extendedProductId)
		{
			XDocument soapRequest = ActivationHelper.CreateSoapRequest(requestType, installationId, extendedProductId);
			HttpWebRequest httpWebRequest = ActivationHelper.CreateWebRequest(soapRequest);
			XDocument soapResponse = new XDocument();
			try
			{
				IAsyncResult asyncResult = httpWebRequest.BeginGetResponse(null, null);
				asyncResult.AsyncWaitHandle.WaitOne();
				using (WebResponse webResponse = httpWebRequest.EndGetResponse(asyncResult))
				{
					using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
					{
						soapResponse = XDocument.Parse(streamReader.ReadToEnd());
					}
				}
				return ActivationHelper.ParseSoapResponse(soapResponse);
			}
			catch
			{
			}
			return "";
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002118 File Offset: 0x00000318
		private static XDocument CreateSoapRequest(int requestType, string installationId, string extendedProductId)
		{
			XElement xelement = new XElement(ActivationHelper.BatchActivationRequestNs + "ActivationRequest", new object[]
			{
				new XElement(ActivationHelper.BatchActivationRequestNs + "VersionNumber", "2.0"),
				new XElement(ActivationHelper.BatchActivationRequestNs + "RequestType", requestType),
				new XElement(ActivationHelper.BatchActivationRequestNs + "Requests", new XElement(ActivationHelper.BatchActivationRequestNs + "Request", new object[]
				{
					new XElement(ActivationHelper.BatchActivationRequestNs + "PID", extendedProductId),
					(requestType == 1) ? new XElement(ActivationHelper.BatchActivationRequestNs + "IID", installationId) : null
				}))
			});
			byte[] bytes = Encoding.Unicode.GetBytes(xelement.ToString());
			string content = Convert.ToBase64String(bytes);
			XDocument result = new XDocument();
			using (HMACSHA256 hmacsha = new HMACSHA256(ActivationHelper.MacKey))
			{
				string content2 = Convert.ToBase64String(hmacsha.ComputeHash(bytes));
				result = new XDocument(new XDeclaration("1.0", "UTF-8", "no"), new object[]
				{
					new XElement(ActivationHelper.SoapSchemaNs + "Envelope", new object[]
					{
						new XAttribute(XNamespace.Xmlns + "soap", ActivationHelper.SoapSchemaNs),
						new XAttribute(XNamespace.Xmlns + "xsi", ActivationHelper.XmlSchemaInstanceNs),
						new XAttribute(XNamespace.Xmlns + "xsd", ActivationHelper.XmlSchemaNs),
						new XElement(ActivationHelper.SoapSchemaNs + "Body", new XElement(ActivationHelper.BatchActivationServiceNs + "BatchActivate", new XElement(ActivationHelper.BatchActivationServiceNs + "request", new object[]
						{
							new XElement(ActivationHelper.BatchActivationServiceNs + "Digest", content2),
							new XElement(ActivationHelper.BatchActivationServiceNs + "RequestXml", content)
						})))
					})
				});
			}
			return result;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002358 File Offset: 0x00000558
		private static HttpWebRequest CreateWebRequest(XDocument soapRequest)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(ActivationHelper.Uri);
			httpWebRequest.Accept = "text/xml";
			httpWebRequest.ContentType = "text/xml; charset=\"utf-8\"";
			httpWebRequest.Headers.Add("SOAPAction", "http://www.microsoft.com/BatchActivationService/BatchActivate");
			httpWebRequest.Host = "activation.sls.microsoft.com";
			httpWebRequest.Method = "POST";
			HttpWebRequest result;
			try
			{
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					soapRequest.Save(requestStream);
				}
				result = httpWebRequest;
			}
			catch
			{
				throw;
			}
			return result;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002400 File Offset: 0x00000600
		private static string ParseSoapResponse(XDocument soapResponse)
		{
			bool flag = soapResponse == null;
			if (flag)
			{
				throw new ArgumentNullException("soapResponse", "The remote server returned an unexpected response.");
			}
			bool flag2 = !soapResponse.Descendants(ActivationHelper.BatchActivationServiceNs + "ResponseXml").Any<XElement>();
			if (flag2)
			{
				throw new Exception("The remote server returned an unexpected response");
			}
			string result;
			try
			{
				XDocument xdocument = XDocument.Parse(soapResponse.Descendants(ActivationHelper.BatchActivationServiceNs + "ResponseXml").First<XElement>().Value);
				bool flag3 = xdocument.Descendants(ActivationHelper.BatchActivationResponseNs + "ErrorCode").Any<XElement>();
				if (flag3)
				{
					string value = xdocument.Descendants(ActivationHelper.BatchActivationResponseNs + "ErrorCode").First<XElement>().Value;
					string text = value;
					string text2 = text;
					uint num = <PrivateImplementationDetails>.ComputeStringHash(text2);
					if (num <= 38089634U)
					{
						if (num != 25696252U)
						{
							if (num != 25843347U)
							{
								if (num != 38089634U)
								{
									goto IL_215;
								}
								if (!(text2 == "0xD5"))
								{
									goto IL_215;
								}
							}
							else
							{
								if (!(text2 == "0x86"))
								{
									goto IL_215;
								}
								return language.getRMValue("InvalidType");
							}
						}
						else
						{
							if (!(text2 == "0x90"))
							{
								goto IL_215;
							}
							return language.getRMValue("IIDError");
						}
					}
					else if (num <= 2205256531U)
					{
						if (num != 1332673248U)
						{
							if (num != 2205256531U)
							{
								goto IL_215;
							}
							if (!(text2 == "0x68"))
							{
								goto IL_215;
							}
							return language.getRMValue("InvalidKey");
						}
						else
						{
							if (!(text2 == "0x7F"))
							{
								goto IL_215;
							}
							return language.getRMValue("Exceeded");
						}
					}
					else if (num != 2255442293U)
					{
						if (num != 2423365578U)
						{
							goto IL_215;
						}
						if (!(text2 == "0x67"))
						{
							goto IL_215;
						}
					}
					else
					{
						if (!(text2 == "0x71"))
						{
							goto IL_215;
						}
						return language.getRMValue("NeverObtained");
					}
					return language.getRMValue("Blocked");
					IL_215:
					result = value;
				}
				else
				{
					bool flag4 = xdocument.Descendants(ActivationHelper.BatchActivationResponseNs + "ResponseType").Any<XElement>();
					if (!flag4)
					{
						throw new Exception("The remote server returned an unrecognized response");
					}
					string value2 = xdocument.Descendants(ActivationHelper.BatchActivationResponseNs + "ResponseType").First<XElement>().Value;
					string text3 = value2;
					string a = text3;
					if (!(a == "1"))
					{
						if (!(a == "2"))
						{
							throw new Exception("The remote server returned an unrecognized response");
						}
						string value3 = xdocument.Descendants(ActivationHelper.BatchActivationResponseNs + "ActivationRemaining").First<XElement>().Value;
						result = value3;
					}
					else
					{
						string value4 = xdocument.Descendants(ActivationHelper.BatchActivationResponseNs + "CID").First<XElement>().Value;
						result = value4;
					}
				}
			}
			catch
			{
				throw;
			}
			return result;
		}

		// Token: 0x04000001 RID: 1
		private static readonly byte[] MacKey = new byte[]
		{
			254,
			49,
			152,
			117,
			251,
			72,
			132,
			134,
			156,
			243,
			241,
			206,
			153,
			168,
			144,
			100,
			171,
			87,
			31,
			202,
			71,
			4,
			80,
			88,
			48,
			36,
			226,
			20,
			98,
			135,
			121,
			160,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};

		// Token: 0x04000002 RID: 2
		private const string Action = "http://www.microsoft.com/BatchActivationService/BatchActivate";

		// Token: 0x04000003 RID: 3
		private static readonly Uri Uri = new Uri("https://activation.sls.microsoft.com/BatchActivation/BatchActivation.asmx");

		// Token: 0x04000004 RID: 4
		private static readonly XNamespace SoapSchemaNs = "http://schemas.xmlsoap.org/soap/envelope/";

		// Token: 0x04000005 RID: 5
		private static readonly XNamespace XmlSchemaInstanceNs = "http://www.w3.org/2001/XMLSchema-instance";

		// Token: 0x04000006 RID: 6
		private static readonly XNamespace XmlSchemaNs = "http://www.w3.org/2001/XMLSchema";

		// Token: 0x04000007 RID: 7
		private static readonly XNamespace BatchActivationServiceNs = "http://www.microsoft.com/BatchActivationService";

		// Token: 0x04000008 RID: 8
		private static readonly XNamespace BatchActivationRequestNs = "http://www.microsoft.com/DRM/SL/BatchActivationRequest/1.0";

		// Token: 0x04000009 RID: 9
		private static readonly XNamespace BatchActivationResponseNs = "http://www.microsoft.com/DRM/SL/BatchActivationResponse/1.0";
	}
}
