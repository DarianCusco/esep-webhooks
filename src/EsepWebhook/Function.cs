using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogInformation($"FunctionHandler received: {input}");

        // Parse the outer API Gateway event
        string jsonString = input.ToString();
        dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonString);

        // Parse the inner GitHub payload from the "body" field
        dynamic body = JsonConvert.DeserializeObject<dynamic>(json.body.ToString());

        // Build payload for Slack
        string payload = $"{{\"text\":\"Issue Created: {body.issue.html_url}\"}}";

        var client = new HttpClient();
        var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        var response = client.Send(webRequest);
        using var reader = new StreamReader(response.Content.ReadAsStream());

        return reader.ReadToEnd();
    }
}
