using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;
using System.Linq;

namespace Rahka.Wuma
{
    public class TelemetryEndpoint
    {
        private readonly CosmosClient _cosmosClient;

        public TelemetryEndpoint(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        [FunctionName("PutTelemetry")]
        public IActionResult Put(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "telemetry")] HttpServerTelemetryModel telemetry,
            [CosmosDB(
                databaseName: "wuma",
                collectionName: "telemetry",
                ConnectionStringSetting = "CosmosDBConnectionString")] out CosmosServerTelemetryModel document,
            ILogger log)
        {
            document = new CosmosServerTelemetryModel
            {
                Id = telemetry.Ip,
                Name = telemetry.Name,
                Type = telemetry.Type,
                TTL = 3600 * 12 * 3
            };

            log.LogInformation($"Telemetry from {document.Type.ToString().ToLower()} server {document.Id} ({document.Name})");

            return new AcceptedResult();
        }

        [FunctionName("GetTelemetryStatistics")]
        public async Task<IActionResult> Statistics(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "telemetry")] HttpRequest request,
            ILogger log)
        {
            var container = _cosmosClient.GetDatabase("wuma").GetContainer("telemetry");

            var iterator = container.GetItemQueryIterator<TelemetryStatistics>(new QueryDefinition("SELECT COUNT(c.type) AS count, c.type FROM c GROUP BY c.type"));
                           
            var statistics = new Dictionary<string, int>();
            while (iterator.HasMoreResults)
            {
                var task = await iterator.ReadNextAsync();
                foreach (var x in task)
                {
                    statistics[x.Type.ToString().ToLower()] = x.Count;
                }           
            }
   
            return new OkObjectResult(statistics);
        }

        public class TelemetryStatistics
        {
            [JsonProperty("type")]
            public ServerType Type { get; set; }

            [JsonProperty("count")]
            public int Count { get; set; }
        }
    }
}
