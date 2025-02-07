using DotNetEnv;
using FiapProcessaVideo.Infrastructure.Messaging.Subscribers;
using FiapProcessaVideo.Application.UseCases;
using FiapProcessaVideo.Infrastructure.Messaging.Model.Shared;
using FiapProcessaVideo.Application.Messaging.Interfaces;
using Amazon.S3;
using Amazon;
using HealthChecks.UI.Client;
using HealthChecks.RabbitMQ;
using FiapProcessaVideo.Infrastructure.Messaging.Publishers;
using Newtonsoft.Json;
using Serilog;
using FiapProcessaVideo.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Env.Load();

//setting listen PORT
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(Convert.ToInt32(Environment.GetEnvironmentVariable("PORT")));
});

//AWS
//---------------------------------------------------------------------------
// Register AWS services with specified options
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    string accessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID").ToString();
    string secretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY").ToString();
    string accessToken = Environment.GetEnvironmentVariable("AWS_ACCESS_TOKEN").ToString();
    string regionEnv = Environment.GetEnvironmentVariable("AWS_REGION").ToString(); // Change to your desired region

    if (string.IsNullOrEmpty(accessKeyId) || string.IsNullOrEmpty(secretAccessKey) || string.IsNullOrEmpty(regionEnv))
    {
        throw new InvalidOperationException("AWS credentials or region information is not set in environment variables.");
    }

    RegionEndpoint region = RegionEndpoint.GetBySystemName(regionEnv);

    var config = new AmazonS3Config
    {
        RegionEndpoint = region, // Set the AWS Region
        UseHttp = false,                        // Use HTTPS (default is false)
        ForcePathStyle = true                   // Enable path-style access (e.g., http://s3.amazonaws.com/bucketname)
    };

    return new AmazonS3Client(accessKeyId, secretAccessKey, accessToken, config);
});

//---------------------------------------------------------------------------
//RabbitMQ configuration
string queue = Environment.GetEnvironmentVariable("AMQP_QUEUE");

// Health checks
var connectionString = Environment.GetEnvironmentVariable("AMQP_URI").ToString();

Console.WriteLine(connectionString);

builder.Services
    .AddHealthChecks()
    .AddRabbitMQ(connectionString, name: "rabbitmq-check", tags: new string[] { "rabbitmq" });

//  Logging
//---------------------------------------------------------------------------
builder.Host.SerilogConfiguration();
//---------------------------------------------------------------------------

builder.Services.Configure<MessagingSubscriberSettings>(options =>
{
    options.QueueName = queue;
    options.Uri = Environment.GetEnvironmentVariable("AMQP_URI").ToString();
});

builder.Services.Configure<MessagingPublisherSettings>(options =>
{
    options.ExchangeName = Environment.GetEnvironmentVariable("AMQP_EXCHANGE").ToString();
    options.QueueName = queue;
    options.Uri = Environment.GetEnvironmentVariable("AMQP_URI").ToString();
});

Console.WriteLine($"Fila: {queue}");

builder.Services.AddScoped<ProcessVideoUseCase>();
builder.Services.AddHostedService<VideoUploadeSubscriber>();
builder.Services.AddScoped<IMessagePublisher, NotificationPublisher>();
//---------------------------------------------------------------------------

var app = builder.Build();

app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = p => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

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
