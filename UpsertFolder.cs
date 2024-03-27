using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Function
{
    public class UpsertFolder
    {
        private readonly ILogger<UpsertFolder> _logger;

        public UpsertFolder(ILogger<UpsertFolder> logger)
        {
            _logger = logger;
        }

        [Function("UpsertFolder")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (requestBody != null)
            {
                _logger.LogInformation(requestBody);

                dynamic? data = JsonConvert.DeserializeObject(requestBody);

                if (data != null && data?.ContainsKey("location"))
                {
                    var location = Convert.ToString(data?["location"]);

                    if (OneDriveHelper.UpsertFolder(location))
                    {
                        return new OkObjectResult("Created folder " + location);
                    }
                    else
                    {
                        return new BadRequestObjectResult("Unable to create folder " + location + "".Trim());
                    }
                }
            }

            return new OkObjectResult("Welcome to Azure Functions UpsertFolder!");
        }
    }
}
