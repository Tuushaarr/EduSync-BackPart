using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;

namespace EduSync.Services
{
    public class EventHubSender
    {
        private readonly EventHubProducerClient _producerClient;

        public EventHubSender(IConfiguration configuration)
        {
            // Read from appsettings.json or Azure App Service config
            string? connectionString = configuration["EventHub:ConnectionString"];
            string? eventHubName = configuration["EventHub:Name"];
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(eventHubName))
                throw new ArgumentException("EventHub connection string or name is missing in configuration.");
            _producerClient = new EventHubProducerClient(connectionString, eventHubName);
        }

        public async Task SendEventAsync(string message)
        {
            using EventDataBatch eventBatch = await _producerClient.CreateBatchAsync();
            if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message))))
                throw new System.Exception("Message too large for the batch.");

            await _producerClient.SendAsync(eventBatch);
        }
    }
}