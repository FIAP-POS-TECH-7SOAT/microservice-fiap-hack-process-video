using DotNetEnv;
using FiapProcessaVideo.Infrastructure.Messaging.Subscribers;
using FiapProcessaVideo.Application.UseCases;
using FiapProcessaVideo.Infrastructure.Messaging.Model.Shared;
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

string jsonQueues = Environment.GetEnvironmentVariable("AMQP_QUEUES");
AmqpQueues queues = new AmqpQueues();
if (!string.IsNullOrEmpty(jsonQueues))
{
    queues = JsonConvert.DeserializeObject<AmqpQueues>(jsonQueues);
    Console.WriteLine(queues.FileQueue.Name);
}

string rabbitmqUsername = Environment.GetEnvironmentVariable("AMQP_USERNAME").ToString();
string rabbitmqPassword = Environment.GetEnvironmentVariable("AMQP_PASSWORD").ToString();
string rabbitmqHostname = Environment.GetEnvironmentVariable("AMQP_HOSTNAME").ToString();
string rabbitmqPort = Environment.GetEnvironmentVariable("AMQP_PORT").ToString();
// Health checks
var connectionString = $"amqps://{rabbitmqUsername}:{rabbitmqPassword}@{rabbitmqHostname}:{rabbitmqPort}";
//var connectionString = "amqps://pfuliwzb:EQFanpERUMVwytUkXu6cmjwZpOuWm57u@jackal.rmq.cloudamqp.com/pfuliwzb";

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
    options.HostName = rabbitmqHostname;
    options.Port = Convert.ToInt32(rabbitmqPort);
    options.UserName = rabbitmqUsername;
    options.Password = rabbitmqPassword;
    options.VirtualHost = Environment.GetEnvironmentVariable("AMQP_VIRTUAL_HOST").ToString();
    options.QueueName = queues.FileQueue.Name;
});

builder.Services.Configure<MessagingPublisherSettings>(options =>
{
    options.HostName = rabbitmqHostname;
    options.Port = Convert.ToInt32(rabbitmqPort);
    options.UserName = rabbitmqUsername;
    options.Password = rabbitmqPassword;
    options.ExchangeName = Environment.GetEnvironmentVariable("AMQP_EXCHANGE").ToString();
    options.VirtualHost = Environment.GetEnvironmentVariable("AMQP_VIRTUAL_HOST").ToString();
    options.RoutingKeys = queues.FileQueue.RoutingKeys.ToList();
    options.QueueName = queues.FileQueue.Name;
});

//builder.Services.AddScoped<IProcessVideoUseCase, ProcessVideoUseCase>();
//builder.Services.AddHostedService<VideoUploadeSubscriber>();
//builder.Services.AddScoped<NotificationPublisher>();
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
