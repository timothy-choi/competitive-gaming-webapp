namespace CompetitiveGamingApp.Controllers;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using CompetitiveGamingApp.Models;
using CompetitiveGamingApp.Services;
using Microsoft.EntityFrameworkCore;
using Elastic.Clients.Elasticsearch;
using StackExchange.Redis;

[ApiController]
[Route("api/Search")]

public class ElasticsearchController : ControllerBase {
    public ElasticsearchController() {
        var cleint = new ElasticClient()
    }
}
