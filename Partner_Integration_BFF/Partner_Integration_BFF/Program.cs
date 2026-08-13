using FluentValidation;
using Partner_Integration_BFF.Contracts;
using Partner_Integration_BFF.Contracts.Requests;
using Partner_Integration_BFF.Messaging;
using Partner_Integration_BFF.Services;
using Partner_Integration_BFF.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IValidator<PartnerTransactionRequest>, TransactionValidator>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
builder.Services.AddSingleton<IPartnerTransactionService, PartnerTransactionService>();
builder.Services
    .AddHttpClient<IVerificationPartner, VerificationPartner>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["PartnerVerification:BaseUrl"]
            ?? "https://localhost:5001/");
    })
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;

        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
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
