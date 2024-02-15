namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using Elastic.Clients.Elasticsearch;
using StackExchange.Redis;
using Elastic.Transport;

[ApiController]
[Route("api/Search")]

public class ElasticsearchController : ControllerBase {
    private readonly ElasticsearchClient _client;
    public ElasticsearchController() {
        _client = new ElasticsearchClient("", new ApiKey(Environment.GetEnvironmentVariable("Elasticsearch_API_Key")!));
    }
}
