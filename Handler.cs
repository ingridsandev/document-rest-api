using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handler
    {
        public async Task<ObjectResult> CreateAsync(Request request)
        {
            Console.WriteLine("Create");
            return new ObjectResult("Create");
        }

        public async Task<ObjectResult> ListAsync(Request request)
        {
            Console.WriteLine("List");
            return new ObjectResult("List");
        }

        public async Task<ObjectResult> DeleteAsync(Request request)
        {
            Console.WriteLine("Delete");
            return new ObjectResult("Delete");
        }

        public async Task<ObjectResult> GetAsync(Request request)
        {
            Console.WriteLine("Get");
            return new ObjectResult("Get");
        }

        public async Task<ObjectResult> UpdateAsync(Request request)
        {
            Console.WriteLine("Update");
            return new ObjectResult("Update");
        }
    }

    public class Request
    {
        public string Key1 {get; set;}
        public string Key2 {get; set;}
        public string Key3 {get; set;}

        public Request(string key1, string key2, string key3)
        {
            Key1 = key1;
            Key2 = key2;
            Key3 = key3;
        }
    }
}
