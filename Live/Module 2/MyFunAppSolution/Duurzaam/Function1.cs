using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Duurzaam
{
    public static class Function1
    {
        [FunctionName("DuurzameFunction")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<Task<string>>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
            outputs.Add(context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
            await context.WaitForExternalEvent<bool>("meppel");
            outputs.Add(context.CallActivityAsync<string>(nameof(SayHello), "London"));

            await Task.WhenAll(outputs);    
            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return string.Join( ',', outputs.Select(t=>t.Result).ToArray());
        }

        [FunctionName(nameof(SayHello))]
        public static async Task<string> SayHello([ActivityTrigger] string name, ILogger log)
        {
            await Task.Delay(5000);
            log.LogInformation("Saying hello to {name}.", name);
            return $"Hello {name}!";
        }

        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DuurzameFunction", "instance");

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}