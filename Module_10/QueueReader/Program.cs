﻿#define SESSIONS

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.ServiceBus;

namespace QueueReader
{
    class Program
    {
        static string EndPoint = "ps-zeur.servicebus.windows.net";
        static (string Name, string KeY) SasKeyReader = ("lezert", "bhrVfqT92GoHDPmBaY+OegCSSyKY8PhKy+ASbDuJcdQ=");
        static string QueueName = "kassa";

        static async Task Main(string[] args)
        {
            await ReadQueueAsync();
            //await ReadQueueProcessorAsync();
            Console.WriteLine("Press Enter to Quit");
            Console.ReadLine();
        }

        private static async Task ReadQueueAsync()
        {
            var nikko = new CancellationTokenSource();
            nikko.CancelAfter(100_000);
            var cred = new AzureNamedKeyCredential(SasKeyReader.Name, SasKeyReader.KeY);
            var client = new ServiceBusClient(EndPoint, cred);
            const int FAIL_NUMBER = 7;
            var rnd = new Random();

            var options = new ServiceBusSessionReceiverOptions { };
            //var opts = new SessionHandlerOptions() { MaxAutoRenewDuration = TimeSpan.FromSeconds(10) };
#if !SESSIONS
            var receiver = client.CreateReceiver(QueueName);
#else
           
            var receiver = await client.AcceptSessionAsync(QueueName, "It's me");

            //var receiver2 = await client.AcceptNextSessionAsync(QueueName);
#endif
            bool exceedDeliveryCount = false;
            do
            {
                //if (nikko.Token.IsCancellationRequested) break;
                var msg = await receiver.ReceiveMessageAsync(TimeSpan.FromMinutes(1));
                
                try
                {
                    if (exceedDeliveryCount || rnd.Next(4, 10) == FAIL_NUMBER)
                    { 
                       // forever = true;
                        throw new Exception("ooops");                 
                    }

                    Console.WriteLine($"Lock Duration: {msg.LockedUntil} Lock Token: {msg.LockToken}");
                    var data = msg.Body.ToString();
                    Console.WriteLine(data);
                    await receiver.CompleteMessageAsync(msg);
                    //
                    //await receiver.RenewMessageLockAsync(msg);
                }
                catch
                {
                    Console.WriteLine("Ooops!!!"); 
                    await receiver.AbandonMessageAsync(msg);
                }
                finally
                {                 
                   
                }
                await Task.Delay(1000);
            }
            while (!nikko.Token.IsCancellationRequested);
            await receiver.CloseAsync();
        }
        private static async Task ReadQueueProcessorAsync()
        {
            var cred = new AzureNamedKeyCredential(SasKeyReader.Name, SasKeyReader.KeY);
            var client = new ServiceBusClient(EndPoint, cred);
            //ServiceBusProcessorOptions opts = new ServiceBusProcessorOptions {  }
            var receiver = client.CreateSessionProcessor(QueueName);
            
            receiver.ProcessMessageAsync += async evtArg => {
                var t1 = Task.Run(async () =>
                {
                    var msg = evtArg.Message;
                    Console.WriteLine($"Lock Duration: {msg.LockedUntil} Lock Token: {msg.LockToken}");
                    var data = msg.Body.ToString();
                    Console.WriteLine($"Van sessie: {msg.SessionId}: {data}");  
                    await evtArg.CompleteMessageAsync(msg);
                });
               // t1.ContinueWith().Wait(10000);
              //await evtArg.RenewSessionLockAsync()
                
                //await Task.Delay(1000);
                //Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            };

            receiver.ProcessErrorAsync += evtArg => {
                Console.WriteLine("Ooops");
                Console.WriteLine(evtArg.Exception.Message);
                return Task.CompletedTask;
            };

            await receiver.StartProcessingAsync();
            Console.WriteLine("Press Enter to quit processing");
            Console.ReadLine();
            await receiver.StopProcessingAsync();

        }
    }
}
