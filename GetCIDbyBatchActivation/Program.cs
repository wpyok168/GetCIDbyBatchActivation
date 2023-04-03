using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCIDbyBatchActivation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //string iid = "209200177437440198194193547128466754585074098382223107026650642";
            string iid = "330512311875727788090139496334835149954645445802270599047875924";
            XmlRequest.MSXmlRequest(1, iid, "00000-04249-038-820384-03-2052-9200.0000-0902023");

            ActivationHelper.CallWebService(1, iid, "00000-04249-038-820384-03-2052-9200.0000-0902023");
            Console.ReadKey();
        }
    }
}
