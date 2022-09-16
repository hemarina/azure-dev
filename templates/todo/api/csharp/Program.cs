using Azure.Identity;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using MongoDB.Driver;
using SimpleTodo.Api;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"]), new DefaultAzureCredential());

var  MyAllowSpecificOrigins = "allowedOrigins";
string[] allowedOrigins = {"https://localhost:3000", "https://ms.portal.azure.com", builder.Configuration["WEB_API_HOST"]};

builder.Services.AddSingleton<ListsRepository>();
builder.Services.AddSingleton(_ => new MongoClient(builder.Configuration[builder.Configuration["AZURE_COSMOS_CONNECTION_STRING_KEY"]]));
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