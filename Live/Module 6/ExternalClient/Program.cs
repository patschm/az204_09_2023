using System.Net.Http.Headers;
using Microsoft.Identity.Client;

internal class Program
{
    private static string ServiceUrl = "https://localhost:8001/";
    private static async Task Main(string[] args)
    {
        //await DoTheCodeFlowAsync();
        await DoTheCredentialFlowAsync();
        
        Console.ReadLine();
    }

    private static async Task DoTheCodeFlowAsync()
    {
        // To make this work do the following:
        // 1) Create an application registration for platform Mobile and Desktop Application.
        //    This prepares Code Grant Flow
        // 2) Set Redirect Uri to http://localhost (must be http. Port is optional)
        var app = PublicClientApplicationBuilder
            .Create("8cc26327-b710-4e74-aca9-ee677fc70093")
            .WithAuthority(AzureCloudInstance.AzurePublic, "030b09d5-7f0f-40b0-8c01-03ac319b2d71")
            .WithRedirectUri("http://localhost:8123");  // http scheme only!

        var token = await app.Build()
        // .AcquireTokenByUsernamePassword
        .AcquireTokenInteractive(
            new string[] { "api://8cc26327-b710-4e74-aca9-ee677fc70093/Lezuhh" })
        .ExecuteAsync();
        Console.WriteLine(token.AccessToken);

        var client = new HttpClient();
        client.BaseAddress = new Uri(ServiceUrl);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        string data = await client.GetStringAsync("weatherforecast");
        Console.WriteLine(data);
    }
    private static async Task DoTheCredentialFlowAsync()
    {
        // To make this work do the following:
        // 1) On the application registration of the webapi define an App Role
        //    for Application. 
        // 2) Create a new Application Registration for the servie app.
        //    a) Certificates & secrets: Generate a new Client Secret
        //    b) API permissions: Add Permission -> My Apis -> Select your webapi registration.
        //    c) Select Application Permissions (if disabled you forgot or wrongly did step 1)
        //    d) Select the roles you defined in webapi registration
        //    e) Grant Admin consent on the newly created permission.
        var app = ConfidentialClientApplicationBuilder
            .Create("8cc26327-b710-4e74-aca9-ee677fc70093")
            .WithTenantId("030b09d5-7f0f-40b0-8c01-03ac319b2d71")
            .WithClientSecret("UpR8Q~22OfOHwUGS3pm~11mM-Q5swtz4Cn6MQboE");

        var token = await app.Build()
            .AcquireTokenForClient(
                new string[]{"api://8cc26327-b710-4e74-aca9-ee677fc70093/.default"}) // Api ID Uri from webapi regstration. Add /.default to it
            .ExecuteAsync();
        Console.WriteLine(token.AccessToken);

        var client = new HttpClient();
        client.BaseAddress = new Uri(ServiceUrl);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);

        string data = await client.GetStringAsync("weatherforecast");
        Console.WriteLine(data);
    }
}