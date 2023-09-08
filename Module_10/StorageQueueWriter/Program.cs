using Azure;
using Azure.Storage.Queues;
using System;
using System.Threading.Tasks;

namespace StorageQueueWriter
{
    class Program
    {
        static string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=pshupstoor;AccountKey=pYllkd7cUYsXpyjLAbEVla51i+OR+9ogG2h121nG5L3UusQwYQGG4hh8zq6dYrLzbEwD+BBnq7S2+ASt2dtaGA==;EndpointSuffix=core.windows.net";
        static string QueueName = "wachtrij";
        static async Task Main(string[] args)
        {
            await WriteToQueueAsync();
            Console.WriteLine("Press Enter to Quit");
            Console.ReadLine();
        }

        private static async Task WriteToQueueAsync()
        {
            //var creed = new AzureSasCredential("sv=2021-06-08&ss=q&srt=so&sp=wa&se=2023-02-17T16:28:41Z&st=2023-02-17T08:28:41Z&spr=https&sig=9bJhfwzZ5LUTnjO87JuuIX4jTXqaYRATVUKJly6KiWk%3D");
            //var client = new QueueClient(new Uri("https://pshubs.queue.core.windows.net/myqueue"), creed );
            var client = new QueueClient(ConnectionString, QueueName);
            for (int i = 0; i < 100; i++)
            {
                await client.SendMessageAsync($"Hello Number {i}", timeToLive:TimeSpan.FromHours(20));
            }
            
        }

    }
}
