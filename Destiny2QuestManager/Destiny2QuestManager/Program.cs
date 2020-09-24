using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace Destiny2QuestManager
{
    class Program
    {
        static string apiKey = "e31cb56a1794452bb3c2b155dd06d19d";
        static string authURL = "https://www.bungie.net/en/OAuth/Authorize";
        static string authClientID = "34117";

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Uses JSON.NET - http://www.nuget.org/packages/Newtonsoft.Json
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-API-Key", apiKey);

                var response = await client.GetAsync("https://www.bungie.net/platform/Destiny/Manifest/InventoryItem/1274330687/");
                var content = await response.Content.ReadAsStringAsync();
                dynamic item = JsonConvert.DeserializeObject(content);

                Console.WriteLine(item.Response.data.inventoryItem.itemName); //Gjallarhorn
            }
        }
    }
}
