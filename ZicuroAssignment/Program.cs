using ZicuroAssignment.Services;

namespace ZicuroAssignment
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new AbxClient();
            var packets = await client.FetchAllPacketsAsync();
            client.WriteToJson(packets);

            Console.WriteLine("Data written to output.json");
         


        }
    }
}
