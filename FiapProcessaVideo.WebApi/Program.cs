using DotNetEnv;
using FiapProcessaVideo.WebApi.Services;
using FiapProcessaVideo.Application.UseCases;
using RabbitMQ.Client;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;

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

// Define AWS credentials and region
var awsOptions = new AWSOptions
{
    Credentials = new Amazon.Runtime.BasicAWSCredentials("access-key-id", "secret-access-key"),
    Region = Amazon.RegionEndpoint.USEast1 // Replace with your region
};

// Register AWS services with specified options
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonS3>();
//RabbitMQ
//---------------------------------------------------------------------------
// string queueName = Environment.GetEnvironmentVariable("AMQP_QUEUE");
// string routingKey = Environment.GetEnvironmentVariable("AMQP_ROUTING_KEY");

// // Add RabbitMQ connection to DI
// var factory = new ConnectionFactory() { Uri = new Uri(Environment.GetEnvironmentVariable("AMQP_URL")) };
// var connection = factory.CreateConnection();

// // Initialize connection and channel
// var channel = connection.CreateModel();

// // Declare queue (optional here; could also be done in the consumer service)
// channel.QueueDeclare(queue: queueName,
//                      durable: false,
//                      exclusive: false,
//                      autoDelete: false,
//                      arguments: null);

// // Bind the queue to the exchange using the routing key
// channel.QueueBind(queue: queueName,
//                   exchange: "amq.direct",
//                   routingKey: routingKey);

//assert bind com a exchange ...

// Register connection and channel in DI container
// builder.Services.AddSingleton<IConnection>(connection);
// builder.Services.AddSingleton<IModel>(channel);
builder.Services.AddScoped<IProcessVideoUseCase, ProcessVideoUseCase>();
// Register RabbitMQ consumer service
// builder.Services.AddHostedService<RabbitMqConsumerService>();
//---------------------------------------------------------------------------

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
