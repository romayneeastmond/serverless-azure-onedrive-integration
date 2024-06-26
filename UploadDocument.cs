using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.Function
{
    public class UploadDocument
    {
        private readonly ILogger<UploadDocument> _logger;

        public UploadDocument(ILogger<UploadDocument> logger)
        {
            _logger = logger;
        }

        [Function("UploadDocument")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (requestBody != null)
            {
                _logger.LogInformation(requestBody);

                dynamic? data = JsonConvert.DeserializeObject(requestBody);

                if (data != null && data?.ContainsKey("document") && data?.ContainsKey("name") && data?.ContainsKey("location"))
                {
                    try
                    {
                        var location = Convert.ToString(data?["location"]);
                        var name = Convert.ToString(data?["name"]);
                        var documentBytes = Convert.FromBase64String(Convert.ToString(data?["document"]));

                        var result = await OneDriveHelper.UploadDocument(location, name, documentBytes);

                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            return new OkObjectResult(new
                            {
                                Id = result,
                                Location = location,
                                Name = name
                            });
                        }
                        else
                        {
                            return new BadRequestObjectResult(new
                            {
                                Error = true,
                                Message = "Unable to upload document"
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        return new BadRequestObjectResult(new
                        {
                            Error = true,
                            e.Message
                        });
                    }
                }
            }

            return new OkObjectResult("Welcome to Azure Functions UploadDocument!");
        }
    }
}
