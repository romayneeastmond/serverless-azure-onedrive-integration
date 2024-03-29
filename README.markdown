# Azure Serverless Functions OneDrive Integration Demo

This Serverless Azure project uses Microsoft Graph to connect to OneDrive to create folders and upload documents. 

## Microsoft Graph API Integration

The OneDrive integration is actually based on granting the application accesss to the Sites.Manage.All and Sites.Read.All permissions. These are needed to connect to the SharePoint documents of the registered application's user (usually an admin account of the Office 365 tenant). 

For convenience create shortcut link to OneDrive from the SharePoint site's document.

Folders and documents will be visible directly from OneDrive or from the registered user's SharePoint instance, e.g. https://registered-app-user.sharepoint.com/Shared%20Documents

## How to Use

Update the hardcoded values in the helper method with a valid Microsoft Entra app registration's AppId, TenantId, and Secret. 

The UpsertFolder endpoint expects a 'location' value passed as a POST, and UploadDocument expects a POST base64 encoded value of a byte array to a 'document' field with 'location' of where the document should be saved on OneDrive, and 'name' for the actual document file name.

Note that the 'location' value can be nested to match an expected folder structure, e.g. "Documents\\2024\\March".

Both endpoints return the Id of either the folder or document.