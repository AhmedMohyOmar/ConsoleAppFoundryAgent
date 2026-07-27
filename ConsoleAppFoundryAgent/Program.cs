//using Azure.Core;
//using Azure.Identity;
//using System.Net.Http.Headers;
//using System.Net.Http.Json;
//using System.Text.Json;

//var tenantId = "664dc553-ec06-475c-ad5e-ef9ff6588edc";
//var clientId = "bfa0d6c0-a881-4ad6-8b2f-8ec8ab0d5921";
//var clientSecret = "zV58Q~ZKvUcRPwPa45dGPR34LstxkXKETuBYrcon";

//var agentApplicationEndpoint =
//    "https://tdra-pocs-resource.services.ai.azure.com/api/projects/tdra-pocs/agents/tdra-cs-agent-poc/endpoint/protocols/openai";

//var credential = new ClientSecretCredential(
//    tenantId: tenantId,
//    clientId: clientId,
//    clientSecret: clientSecret);

//// Foundry uses the ai.azure.com token scope.
//var token = await credential.GetTokenAsync(
//    new TokenRequestContext(new[] { "https://ai.azure.com/.default" }));

//using var httpClient = new HttpClient();

//httpClient.DefaultRequestHeaders.Authorization =
//    new AuthenticationHeaderValue("Bearer", token.Token);

//var requestUrl =
//    $"{agentApplicationEndpoint.TrimEnd('/')}/responses?api-version=2025-11-15-preview";

//var requestBody = new
//{
//    input = "Hello, can you introduce yourself and explain what you can help with?"
//};

//var response = await httpClient.PostAsJsonAsync(requestUrl, requestBody);

//var responseJson = await response.Content.ReadAsStringAsync();

//if (!response.IsSuccessStatusCode)
//{
//    Console.WriteLine("Request failed.");
//    Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
//    Console.WriteLine(responseJson);
//    return;
//}

//using var jsonDocument = JsonDocument.Parse(responseJson);

//var outputText = ExtractOutputText(jsonDocument.RootElement);

//Console.WriteLine("Agent response:");
//Console.WriteLine(outputText);

//static string ExtractOutputText(JsonElement root)
//{
//    // Some Responses API clients expose output_text directly.
//    if (root.TryGetProperty("output_text", out var outputTextElement))
//    {
//        return outputTextElement.GetString() ?? string.Empty;
//    }

//    // Fallback parser for output[] -> content[] -> text
//    if (!root.TryGetProperty("output", out var outputArray))
//    {
//        return root.ToString();
//    }

//    var parts = new List<string>();

//    foreach (var outputItem in outputArray.EnumerateArray())
//    {
//        if (!outputItem.TryGetProperty("content", out var contentArray))
//        {
//            continue;
//        }

//        foreach (var contentItem in contentArray.EnumerateArray())
//        {
//            if (contentItem.TryGetProperty("text", out var textElement))
//            {
//                var text = textElement.GetString();
//                if (!string.IsNullOrWhiteSpace(text))
//                {
//                    parts.Add(text);
//                }
//            }
//        }
//    }

//    return string.Join(Environment.NewLine, parts);
//}


using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

var tenantId = "664dc553-ec06-475c-ad5e-ef9ff6588edc";
var clientId = "bfa0d6c0-a881-4ad6-8b2f-8ec8ab0d5921";
var clientSecret = "zV58Q~ZKvUcRPwPa45dGPR34LstxkXKETuBYrcon";

var agentApplicationEndpoint =
    "https://tdra-pocs-resource.services.ai.azure.com/api/projects/tdra-pocs/agents/tdra-cs-agent-poc/endpoint/protocols/openai";

// 1. Define your dynamic parameters
var caseDetails = "Customer filed a claim regarding delayed service installation exceeding 14 business days without prior notification.";
var providerName = "Telecom Service Provider X";
var providerResponse = "Installation was delayed due to unexpected fiber infrastructure maintenance in the customer sector.";

// 2. Format the prompt payload
var userPrompt = $"""
Please review the following case details and provide a suggested decision based on our uploaded knowledge base documents:

- Case Details: {caseDetails}
- Service Provider Name: {providerName}
- Service Provider Response: {providerResponse}
""";

var credential = new ClientSecretCredential(
    tenantId: tenantId,
    clientId: clientId,
    clientSecret: clientSecret);

// Foundry uses the ai.azure.com token scope
var token = await credential.GetTokenAsync(
    new TokenRequestContext(new[] { "https://ai.azure.com/.default" }));

using var httpClient = new HttpClient();

httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token.Token);

var requestUrl =
    $"{agentApplicationEndpoint.TrimEnd('/')}/responses?api-version=2025-11-15-preview";

// 3. Build payload with formatted dispute input
var requestBody = new
{
    input = userPrompt
};

var response = await httpClient.PostAsJsonAsync(requestUrl, requestBody);

var responseJson = await response.Content.ReadAsStringAsync();

if (!response.IsSuccessStatusCode)
{
    Console.WriteLine("Request failed.");
    Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
    Console.WriteLine(responseJson);
    return;
}

using var jsonDocument = JsonDocument.Parse(responseJson);

var outputText = ExtractOutputText(jsonDocument.RootElement);

Console.WriteLine("--- Suggested Decision ---");
Console.WriteLine(outputText);

static string ExtractOutputText(JsonElement root)
{
    // Check direct output_text property
    if (root.TryGetProperty("output_text", out var outputTextElement))
    {
        return outputTextElement.GetString() ?? string.Empty;
    }

    // Fallback parser for output[] -> content[] -> text
    if (!root.TryGetProperty("output", out var outputArray))
    {
        return root.ToString();
    }

    var parts = new List<string>();

    foreach (var outputItem in outputArray.EnumerateArray())
    {
        if (!outputItem.TryGetProperty("content", out var contentArray))
        {
            continue;
        }

        foreach (var contentItem in contentArray.EnumerateArray())
        {
            if (contentItem.TryGetProperty("text", out var textElement))
            {
                var text = textElement.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    parts.Add(text);
                }
            }
        }
    }

    return string.Join(Environment.NewLine, parts);
}