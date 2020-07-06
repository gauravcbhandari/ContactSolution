using System;
using System.Collections.Generic;
using System.IO;
using Azure.Storage.Blobs;
using ContactSolution.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading;
using System.Threading.Tasks;

namespace ContactSolution
{
    class TableManager : ITableManager
    {
        private CloudTable mytable = null;
        private readonly IConfiguration _config;

        private readonly string _connectionString;
        public TableManager(IConfiguration config)
        {
            _config = config;
            _connectionString = GetConnectionStringFromVault().GetAwaiter().GetResult();
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();
            mytable = cloudTableClient.GetTableReference("Contacts");
            mytable.CreateIfNotExistsAsync();
        }

        private async Task<string> GetConnectionStringFromVault()
        {
            AzureServiceTokenProvider azureServiceTokenProvider =
               new AzureServiceTokenProvider();

            KeyVaultClient keyVaultClient =
            new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            var secret = await keyVaultClient
                .GetSecretAsync("https://contactsvault.vault.azure.net/secrets/StorageAccountConnectionString/884a1252d73442ebb8ce1cf194f26c70")
                        .ConfigureAwait(false);

            return secret.Value;
        }

        public IEnumerable<Contact> GetAll()
        {
            var query = new TableQuery<Contact>();
            var entties = mytable.ExecuteQuery(query);
            return entties;
        }

        public void CreateOrUpdate(Contact myTableOperation)
        {
            var operation = TableOperation.InsertOrReplace(myTableOperation);
            mytable.Execute(operation);
        }

        public void UploadFileToBlob(IFormFile files)
        {
            BlobServiceClient image = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = image.GetBlobContainerClient("photos");
            string filename = Path.GetFileName(files.FileName);
            containerClient.UploadBlob(filename, files.OpenReadStream());
        }
    }
}