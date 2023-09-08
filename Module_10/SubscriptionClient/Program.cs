﻿using Azure;
using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace SubscriptionClient
{
    class Program
    {
        static string EndPoint = "ps-zeur.servicebus.windows.net";
        static (string Name, string KeY) SasKeyReader = ("Lezert", "OEgqgifb7przdAi8ZRkEoMyTd9R5votn++ASbFEsiKk=");
        static string TopicName = "onderwerp";

        static async Task Main(string[] args)
        {
            //await ReadQueueAsync();
            await ReadQueueProcessorAsync();
            Console.WriteLine("Press Enter to Quit");
            Console.ReadLine();
        }

        private static async Task ReadQueueAsync()
        {
            var cred = new AzureNamedKeyCredential(SasKeyReader.Name, SasKeyReader.KeY);
            var client = new ServiceBusClient(EndPoint, cred);
            var receiver = client.CreateReceiver(TopicName, "Support");
            do
            {
                var msg = await receiver.ReceiveMessageAsync();
                Console.WriteLine($"Lock Duration: {msg.LockedUntil} Lock Token: {msg.LockToken}");
                var data = msg.Body.ToString();
                //msg.ApplicationProperties.
                Console.WriteLine(data);
               // await receiver.CompleteMessageAsync(msg);
                //await receiver.AbandonMessageAsync(msg);
                //await receiver.RenewMessageLockAsync(msg);
                await Task.Delay(1000);
            }
            while (true);
        }
        private static async Task ReadQueueProcessorAsync()
        {
            var supportReceiver = CreateProcessor("Support"); 
            var salesReceiver = CreateProcessor("Sales");
            var ceoReceiver = CreateProcessor("Management");

            await supportReceiver.StartProcessingAsync();
            await salesReceiver.StartProcessingAsync();
            await ceoReceiver.StartProcessingAsync();
            Console.WriteLine("Press Enter to quit processing");
            Console.ReadLine();
            await supportReceiver.StopProcessingAsync();
            await salesReceiver.StopProcessingAsync();
            await ceoReceiver.StopProcessingAsync();

        }

        private static ServiceBusProcessor CreateProcessor(string subscription)
        {
            var cred = new AzureNamedKeyCredential(SasKeyReader.Name, SasKeyReader.KeY);
            var client = new ServiceBusClient(EndPoint, cred);
            var processor = client.CreateProcessor(TopicName, subscription);
            processor.ProcessMessageAsync += async evtArg => {
                Console.WriteLine($"{subscription} receiced: {evtArg.Message.Body.ToString()}");
                //hrow new Exception("Ooops");
                //Console.WriteLine(evtArg.Message.Body.ToString());
                await evtArg.CompleteMessageAsync(evtArg.Message);
               // return Task.CompletedTask;
            };
            processor.ProcessErrorAsync += evtArg => {
                Console.WriteLine($"{subscription} fout");
                Console.WriteLine(evtArg.Exception.Message);
                return Task.CompletedTask;
            };
            return processor;
        }
    }
}
