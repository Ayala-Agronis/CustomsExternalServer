using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Permissions; //.SecurityPermission; 

namespace CustomsExternal.Services
{
    public class RexTester
    {
        public class Program
        {
            public static void Main(string[] args)
            {

                System.Net.ServicePointManager.DefaultConnectionLimit = 100;

                var url = "<<please replace this with service endpoint>>";

                var json = "{<request json}";
                using (var client = CreateClient(url))
                {
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var res = client.PostAsync(url, httpContent).Result;

                    Console.WriteLine(res.Content.ReadAsStringAsync().Result);
                }
            }


            private static HttpClient CreateClient(string p_url)
            {
                var client = new HttpClient() { };
                client.DefaultRequestHeaders.Accept.Add(
                 new MediaTypeWithQualityHeaderValue("application/json"));

                var mb = new MB().Build();

                client.DefaultRequestHeaders.Add("X-tranzila-api-app-key", mb.publicK);
                client.DefaultRequestHeaders.Add("X-tranzila-api-request-time", mb.unixTS.ToString());
                client.DefaultRequestHeaders.Add("X-tranzila-api-nonce", mb.hex);
                client.DefaultRequestHeaders.Add("X-tranzila-api-access-token", mb.access_token);
                Console.WriteLine(mb.unixTS.ToString());
                Console.WriteLine(mb.hex);
                Console.WriteLine(mb.access_token);

                return client;
            }

            public class MB
            {

                public string publicK = "zNEU0gfuD7msqpZXec0tu2bY2KysGMFA2BENR3lGHFy0KUqwhnNRnhSDoV9JkCo5yE6iNOjIf0b";
                public long unixTS
                {
                    get;
                    set;
                }
                public string hex
                {
                    get;
                    set;
                }
                public string access_token
                {
                    get;
                    set;
                }
                Random rnd = new Random();
                string key = "mBYaIcKMl6";

                public MB Build()
                {
                    Byte[] b = new Byte[40];
                    rnd.NextBytes(b);
                    hex = ByteToString(b);
                    unixTS = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    var encoding = Encoding.UTF8;
                    var message = key + unixTS + hex;

                    byte[] keyByte = encoding.GetBytes(publicK);
                    byte[] messageBytes = encoding.GetBytes(message);
                    using (var hmacsha256 = new HMACSHA256(messageBytes))
                    {
                        byte[] hashmessage = hmacsha256.ComputeHash(keyByte);
                        access_token = ByteToString(hashmessage);
                    }

                    return this;
                }

                string ByteToString(byte[] buff)
                {
                    string sbinary = "";

                    for (int i = 0; i < buff.Length; i++)
                    {
                        sbinary += buff[i].ToString("X2"); // hex format
                    }
                    return (sbinary.TrimEnd());
                }


                public string bin2Hex(string strBin)

                {

                    int decNumber = bin2Dec(strBin);

                    return dec2Hex(decNumber);

                }
                public int bin2Dec(string strBin)
                {
                    return Convert.ToInt16(strBin, 2);

                }
                private string dec2Hex(int val)

                {

                    return val.ToString("X");
                }
            }
        }
    }
}