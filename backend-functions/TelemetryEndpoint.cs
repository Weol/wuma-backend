using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;

namespace Rahka.Wuma
{
    public class TelemetryEndpoint
    {
        private readonly Container _container;

        public TelemetryEndpoint(CosmosClient cosmosClient)
        {
            _container = cosmosClient.GetDatabase("wuma").GetContainer("telemetry");
        }

        [FunctionName("PutTelemetry")]
        public async Task<IActionResult> Put(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "telemetry")] HttpServerTelemetryModel telemetry,
            ILogger log)
        {
            var model = new CosmosServerTelemetryModel
            {
                Id = telemetry.Ip,
                Name = telemetry.Name,
                Type = telemetry.Type,
                TTL = 3600 * 12 * 3
            };

            await _container.UpsertItemAsync<CosmosServerTelemetryModel>(model);

            log.LogInformation($"Telemetry from {model.Type.ToString().ToLower()} server {model.Id} ({model.Name})");

            return new AcceptedResult();
        }

        [FunctionName("GetTelemetryStatistics")]
        public async Task<IActionResult> Statistics(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "telemetry")] HttpRequest request,
            ILogger log)
        {
            var iterator = _container.GetItemQueryIterator<TelemetryStatistics>(new QueryDefinition("SELECT COUNT(c.type) AS count, c.type FROM c GROUP BY c.type"));
                           
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
