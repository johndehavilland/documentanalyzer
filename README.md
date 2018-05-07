# Azure Intelligent Document Analyzer.

This application supports analyzing documents, using Azure Cognitive Services to extract additional information such as entities, handwriting to text and images to text, and image captioning for a richer search experience.

Currently this demo supports:
* PDF - text based
* PDF - image based
* Images

## Setup

1. Deploy the infrastructure components:
    <a href="https://ms.portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fjohndehavilland%2Fdocumentanalyzer%2Fmaster%2Fazure-deploy.json" target="_blank">
        <img src="http://azuredeploy.net/deploybutton.png"/>
    </a>

2. Download the latest FunctionApp zip file and WebApp zip file from the releases folder in this repo.
3. Navigate to the FunctionApp code deploy page: https://<funtion_name>.scm.azurewebsites.net/ZipDeploy
4. Drag and drop the FunctionApp zip file into the file area on the ZipDeploy page.
5. Navigate to the WebApp code deploy page: https://<webapp_name>.scm.azurewebsites.net/ZipDeploy
6. Drag and drop the WebApp zip file into the file area on the ZipDeploy page.
7. In the Azure portal, go to the Web App and find the Authentication tab.
8. Under the authentication Tab setup authentication. Change *Action to take when request to authenticated* to **Azure Active Directory**. 
9. Choose Azure Active Directory and **Express**. This will prompt you to create a new Azure AD App - keep the default settings and press Save.
10. Make sure to save your authentication credentials.
11. Now navigate to your website. It should prompt you with an Authentication screen and an Authorization screen.
12. After logging in you should be able to test out the document upload process by uploading a document via the UI. After a minute or so this document should be searchable.

## Additional Notes
You can upload single or multiple documents via the front end website. If you want to upload a large number of documents more efficiently then you can do it directly to the documents container of the storage account using [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/).

