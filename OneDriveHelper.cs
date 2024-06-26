using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using DriveUpload = Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;

namespace Application.Function
{
    public static class OneDriveHelper
    {
        public async static Task<string> CreateFolder(GraphServiceClient graphServiceClient, string defaultId, string folderId, string location)
        {
            try
            {
                var requestBody = new DriveItem
                {
                    Name = location,
                    Folder = new Folder
                    {
                    },
                    AdditionalData = new Dictionary<string, object>
                    {
                        {
                            "@microsoft.graph.conflictBehavior" , "fail" //fail, replace, or rename
                        },
                    },
                };

                var result = await graphServiceClient.Drives[defaultId].Items[folderId].Children.PostAsync(requestBody);

                return result?.Id ?? string.Empty;
            }
            catch (ODataError e)
            {
                Console.WriteLine($"Error creating folder: {e.Error?.Message}");
            }

            return string.Empty;
        }

        public async static Task<string> GetDefaultDriveId(GraphServiceClient graphServiceClient)
        {
            try
            {
                var drives = await graphServiceClient.Drives.GetAsync();

                var defaultId = drives?.Value?[0].Id;

                return defaultId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

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

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);

            var graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphServiceClient;
        }

        public async static Task<string> UploadDocument(string location, string name, byte[] byteArray)
        {
            var graphServiceClient = GetGraphServiceClient();

            var defaultId = await GetDefaultDriveId(graphServiceClient);

            return await UploadDocument(graphServiceClient, defaultId, location, name, byteArray);
        }

        public async static Task<string> UploadDocument(GraphServiceClient graphServiceClient, string defaultId, string location, string name, byte[] byteArray)
        {
            if (string.IsNullOrWhiteSpace(defaultId) || string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(name) || byteArray == null)
            {
                return string.Empty;
            }

            var folderId = await UpsertFolder(graphServiceClient, defaultId, location);

            if (string.IsNullOrWhiteSpace(folderId))
            {
                return string.Empty;
            }

            var uploadSessionRequestBody = new DriveUpload.CreateUploadSessionPostRequestBody
            {
                Item = new DriveItemUploadableProperties
                {
                    AdditionalData = new Dictionary<string, object>
                {
                    {
                        "@microsoft.graph.conflictBehavior", "replace" //fail, replace, or rename
                    },
                },
                },
            };

            using var memorySteam = new MemoryStream(byteArray);

            var uploadSession = await graphServiceClient.Drives[defaultId].Items[folderId].ItemWithPath(name).CreateUploadSession.PostAsync(uploadSessionRequestBody);

            int maxSliceSize = 320 * 1024;
            var fileUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, memorySteam, maxSliceSize, graphServiceClient.RequestAdapter);

            var totalLength = memorySteam.Length;

            var progress = new Progress<long>(prog =>
            {
                Console.WriteLine($"Uploaded {prog} bytes of {totalLength} bytes");
            });

            try
            {
                var result = await fileUploadTask.UploadAsync(progress);

                return result?.ItemResponse?.Id ?? string.Empty;
            }
            catch (ODataError e)
            {
                Console.WriteLine($"Error uploading: {e.Error?.Message}");
            }

            return string.Empty;
        }

        public async static Task<string> UpsertFolder(string location)
        {
            var graphServiceClient = GetGraphServiceClient();

            var defaultId = await GetDefaultDriveId(graphServiceClient);

            return await UpsertFolder(graphServiceClient, defaultId, location);
        }

        public async static Task<string> UpsertFolder(GraphServiceClient graphServiceClient, string defaultId, string location)
        {
            if (string.IsNullOrWhiteSpace(defaultId) || string.IsNullOrWhiteSpace(location))
            {
                return string.Empty;
            }

            var folderId = "root";
            var directoryParts = location.Split('\\', StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < directoryParts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(folderId))
                {
                    break;
                }

                var folder = directoryParts[i].Trim();
                var children = await graphServiceClient.Drives[defaultId].Items[folderId].Children.GetAsync();

                if (children == null)
                {
                    folderId = await CreateFolder(graphServiceClient, defaultId, folderId, folder);
                }
                else if (children != null && children.Value != null)
                {
                    var foundChildInFolder = false;

                    foreach (var child in children.Value)
                    {
                        if (child.Name?.ToLower().Trim() == folder.ToLower())
                        {
                            folderId = child.Id;
                            foundChildInFolder = true;

                            break;
                        }
                    }

                    if (foundChildInFolder == false)
                    {
                        folderId = await CreateFolder(graphServiceClient, defaultId, folderId ?? string.Empty, folder);
                    }
                }
            }

            return folderId ?? string.Empty;
        }
    }
}