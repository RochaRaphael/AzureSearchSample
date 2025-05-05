using SearchIA.Application.Services;
using SearchIA.Application;
using Microsoft.OpenApi.Models;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;
using Azure.AI.OpenAI;
using Azure;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddScoped<SearchAIService>();
builder.Services.AddSingleton<OpenAIClient>(s =>
{
    var settings = s.GetRequiredService<IOptions<ApiSettings>>().Value;
    var uri = new Uri(settings.OpenAIEndpoint);
    var credential = new AzureKeyCredential(settings.OpenAIKey);
    return new AzureOpenAIClient(uri, credential);
});

builder.Services.AddSingleton<SearchClient>(s =>
{
    var apiSettings = s.GetRequiredService<IOptions<ApiSettings>>().Value;
    var config = s.GetRequiredService<IConfiguration>();
    var endpoint = apiSettings.AzureSearchEndpoint;
    var indexName = apiSettings.AzureSearchIndexName;
    var apiKey = apiSettings.AzureSearchKey;

    return new SearchClient(new Uri(endpoint), indexName, new Azure.AzureKeyCredential(apiKey));
});

builder.Services.AddSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway SearchIA", Version = "v1" });

    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
    foreach (var xmlFile in xmlFiles)
    {
        setup.IncludeXmlComments(xmlFile);
    }
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();


