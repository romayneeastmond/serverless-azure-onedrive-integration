using Azure.Identity;
using Microsoft.Graph;

namespace Application.Function
{
    public static class OneDriveHelper
    {
        public static GraphServiceClient GetGraphServiceClient()
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var clientId = "";
            var tenantId = "";
            var clientSecret = "";

            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            var graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphServiceClient;
        }

        public static bool UpsertFolder(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return false;
            }

            return false;
        }

        public static bool UploadDocument(string location, byte[] document)
        {
            if (string.IsNullOrWhiteSpace(location) || document == null)
            {
                return false;
            }

            return false;
        }
    }
}