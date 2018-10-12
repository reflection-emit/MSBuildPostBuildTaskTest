using Cauldron;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security;

namespace EncryptTester
{
    public class Rootobject
    {
        public string bla { get; set; }

        public string config { get; set; }

        public string date { get; set; }

        [JsonConverter(typeof(EncryptedToSecureString))]
        public SecureString password { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var file = JsonConvert.DeserializeObject<Rootobject>( File.ReadAllText("test.json") );

            Console.WriteLine(file.password.GetString());
            Console.ReadLine();
        }
    }
}