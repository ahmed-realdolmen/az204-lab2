using AccessBlobStorage.Options;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration.GetSection("StorageAccount").Get<StorageAccountOptions>();
builder.Services.AddScoped(_ =>
{
    return new BlobServiceClient(new Uri(config.Uri), new ClientSecretCredential(config.TenantId, config.CientId, config.Secret));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
