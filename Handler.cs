using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handler
    {
        private readonly IMongoCollection<Document> collection;

        public Handler()
        {
            var mongoClient = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING"));
            collection = mongoClient.GetDatabase("Db-Documents").GetCollection<Document>("Documents");
        }

        public async Task<APIGatewayProxyResponse> CreateAsync(APIGatewayProxyRequest request)
        {
            // Convert string body to Document
            var document = JsonConvert.DeserializeObject<Document>(request.Body);

            // Insert document
            await collection.InsertOneAsync(document);

            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(document)
            };
        }

        public async Task<APIGatewayProxyResponse> ListAsync(APIGatewayProxyRequest request)
        {
            Console.WriteLine("request" + JsonConvert.SerializeObject(request));
            
            request.QueryStringParameters.TryGetValue("Page", out string sPage);
            request.QueryStringParameters.TryGetValue("PageSize", out string sPageSize);
            var page = int.Parse(sPage);
            var pageSize = int.Parse(sPageSize);
            
            var query = collection.Find(Builders<Document>.Filter.Empty);

            var totalTask = query.CountAsync();

            var itemsTask = query.Skip(page * pageSize).Limit(pageSize).ToListAsync();

            await Task.WhenAll(totalTask, itemsTask);

            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(new
                {
                    Total = totalTask.Result,
                    Items = itemsTask.Result
                })
            };
        }

        public async Task<APIGatewayProxyResponse> DeleteAsync(APIGatewayProxyRequest request)
        {
            Console.WriteLine("request: " + JsonConvert.SerializeObject(request));

            // Convert body string to Document
            var document = JsonConvert.DeserializeObject<Document>(request.Body);

            Console.WriteLine("document: " + JsonConvert.SerializeObject(document));

            var filter = Builders<Document>.Filter.And(
                Builders<Document>.Filter.Eq(n => n.Id, document.Id));

            var response = await collection.FindOneAndDeleteAsync(filter);
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(response)
            };
        }

        public async Task<APIGatewayProxyResponse> GetAsync(APIGatewayProxyRequest request)
        {
            Console.WriteLine("request: " + JsonConvert.SerializeObject(request));

            request.PathParameters.TryGetValue("id", out string id);

            var query = collection.Find(Builders<Document>.Filter.Eq(n => n.Id, ObjectId.Parse(id)));

            var totalTask = query.CountAsync();

            var itemsTask = query.ToListAsync();

            await Task.WhenAll(totalTask, itemsTask);

            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(new
                {
                    Total = totalTask.Result,
                    Items = itemsTask.Result
                })
            };
        }

        public async Task<APIGatewayProxyResponse> UpdateAsync(APIGatewayProxyRequest request)
        {
            Console.WriteLine("request: " + JsonConvert.SerializeObject(request));

            request.PathParameters.TryGetValue("id", out string id);

            // Convert body string to Document
            var document = JsonConvert.DeserializeObject<Document>(request.Body);

            var response = await collection.FindOneAndUpdateAsync(
                Builders<Document>.Filter.Where(d => d.Id == document.Id),
                Builders<Document>.Update
                    .Set(d => d.Description, document.Description)
                    .Set(d => d.UpdatedAt, DateTime.Now),
                new FindOneAndUpdateOptions<Document>
                {
                    ReturnDocument = ReturnDocument.After
                });

            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(response)
            };
        }
    }

    public class Document
    {
        public Document(ObjectId id, string description)
        {
            Id = id;
            Description = description;
        }

        public ObjectId Id { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [BsonElement("description")]
        public string Description { get; set; }
    }

    public class ListRequest
    {
        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
