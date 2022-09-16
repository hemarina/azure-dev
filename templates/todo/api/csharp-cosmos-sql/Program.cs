using Azure.Identity;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Azure.Cosmos;
using SimpleTodo.Api;

var credential = new DefaultAzureCredential();
var builder = WebApplication.CreateBuilder(args);

var  MyAllowSpecificOrigins = "allowedOrigins";
string[] allowedOrigins = {"https://localhost:3000", "https://ms.portal.azure.com", builder.Configuration["WEB_API_HOST"]};


builder.Services.AddSingleton<ListsRepository>();
builder.Services.AddSingleton(_ => new CosmosClient(builder.Configuration["AZURE_COSMOS_ENDPOINT"], credential, new CosmosClientOptions()
{
    SerializerOptions = new CosmosSerializationOptions
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    }
}));
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
                      });
});
builder.Services.AddControllers();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

// Swagger UI
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("./openapi.yaml", "v1");
    options.RoutePrefix = "";
});

app.UseStaticFiles(new StaticFileOptions{
    // Serve openapi.yaml file
    ServeUnknownFileTypes = true,
});

app.MapControllers();
app.Run();