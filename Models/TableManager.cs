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
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace ContactSolution
{
    class TableManager : ITableManager
    {
        private CloudTable mytable = null;
        private readonly IConfiguration _config;

        private readonly string _blobConnectionString;

        private readonly string _queueConnectionString;

        static QueueClient queueClient;
        public TableManager(IConfiguration config)
        {
            _config = config;
            _blobConnectionString = GetConnectionStringFromVault("StorageAccount").GetAwaiter().GetResult();
            _queueConnectionString = GetConnectionStringFromVault("Queue").GetAwaiter().GetResult();
            var storageAccount = CloudStorageAccount.Parse(_blobConnectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();
            mytable = cloudTableClient.GetTableReference("Contacts");
            mytable.CreateIfNotExistsAsync();
        }

        private async Task<string> GetConnectionStringFromVault(string resourceName)
        {
            AzureServiceTokenProvider azureServiceTokenProvider =
               new AzureServiceTokenProvider();

            KeyVaultClient keyVaultClient =
            new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            var secret = await keyVaultClient
                .GetSecretAsync(_config[resourceName])
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
            BlobServiceClient image = new BlobServiceClient(_blobConnectionString);
            BlobContainerClient containerClient = image.GetBlobContainerClient("photos");
            string filename = Path.GetFileName(files.FileName);
            containerClient.UploadBlob(filename, files.OpenReadStream());
        }

        public void SendMessageToQueue(Contact contact)
        {
            string sbQueueName = "contact";

            string messageBody = string.Empty;
            messageBody = contact.FirstName + " - " + contact.LastName + " - " + contact.CellNo + " - " + contact.Photo;
            queueClient = new QueueClient(_queueConnectionString, sbQueueName);

            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            Console.WriteLine($"Message Added in Queue: {messageBody}");

            DateTimeOffset scheduleTime = DateTime.UtcNow.AddSeconds(30);
            queueClient.ScheduleMessageAsync(message, scheduleTime);
        }
    }
}