using StrategyFactoryPattern.Application.Notifications;
using StrategyFactoryPattern.Application.Notifications.Models;
using StrategyFactoryPattern.Infrastructure.Notifications;
using StrategyFactoryPattern.Infrastructure.Notifications.SendGrid;
using StrategyFactoryPattern.Infrastructure.Notifications.Smtp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<INotificationsStrategy, NotificationService>();
builder.Services.AddScoped<INotificationsFactory, NotificationsFactory>();
builder.Services.AddScoped<SendGridNotificationService>();
builder.Services.AddScoped<SmtpNotificationsService>();

builder.Services.Configure<NotificationsConfig>(builder.Configuration.GetSection("Notifications"));

var app = builder.Build();

app.MapPost("/send", async (SendNotificationRequest request, INotificationsStrategy notificationService) =>
{
    var response = await notificationService.SendNotification(request);

    return response.IsSuccess ? Results.Ok(response) : Results.BadRequest(response);
});

app.Run();
