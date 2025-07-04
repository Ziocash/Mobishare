using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Mobishare.Core.Data;
using Mobishare.Core.Security;
using Mobishare.Core.Security.Policies;
using Mobishare.Core.Requests;
using Mobishare.Infrastructure.Services.HostedServices;
using System.Reflection;
using Mobishare.Infrastructure.Services.MQTT;
using Mobishare.App.Services;
using Mobishare.Core.Services.GoogleGeocoding;
using Mobishare.Infrastructure.Services.SignalR;
using PayPal.REST.Client;
using PayPal.REST.Models;
using Mobishare.Infrastructure.Services.ChatBotAIService;
// using Mobishare.Infrastructure.Services.ChatBotAIService.IntentClassifier;
// using Mobishare.Infrastructure.Services.ChatBotAIService.IntentRouter;
using Mobishare.Core.Services.UserContext;
using Microsoft.SemanticKernel;
using Mobishare.Infrastructure.Services.ChatBotAIService.Pulgins;
using Mobishare.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllers();


#region SignalR configuration
builder.Services.AddSignalR();
#endregion

#region Google geocoding service configuration
builder.Services.AddScoped<IGoogleGeocodingService, GoogleGeocodingService>();
#endregion

#region Connection to the database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
#endregion

#region Identity configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
#endregion

#region MediatR configuration
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(IQueryService).Assembly, Assembly.GetExecutingAssembly()));
#endregion

#region AutoMapper configuration
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
#endregion

#region MQTT configuration
builder.Services.AddSingleton<MqttMessageHandler>();
builder.Services.AddHostedService<MqttHostedService>();
#endregion

#region Google Authentication
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
});
#endregion

#region Authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PolicyNames.IsAdmin, p => p.AddRequirements(new IsAdmin()))
    .AddPolicy(PolicyNames.IsStaff, p => p.AddRequirements(new IsStaff()))
    .AddPolicy(PolicyNames.IsTechnician, p => p.AddRequirements(new IsTechnician()));

builder.Services.AddScoped<IAuthorizationHandler, IsAdminAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsStaffAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsTechnicianAuthorizationHandler>();
#endregion


builder.Services.AddScoped<SemanticRouterService>(sp => 
    new SemanticRouterService(
        sp.GetRequiredService<Kernel>(), 
        sp.GetRequiredService<ILogger<SemanticRouterService>>()
    )
);

builder.Services.AddScoped<Kernel>(sp =>
{
    var config = builder.Configuration.GetSection("Ollama:Llm");
    var urlApiClient = config["UrlApiClient"] ?? throw new Exception("Manca l'URL per Ollama");
    var modelName = config["ModelName"] ?? throw new Exception("Manca il nome del modello");
   
    #pragma warning disable SKEXP0070 // disabilita warning sperimentali
    var kernelBuilder = Kernel.CreateBuilder();
    // Usa Ollama come servizio di completamento
    kernelBuilder.AddOllamaChatCompletion(
        modelId: modelName,
        endpoint: new Uri(urlApiClient),
        serviceId: "ollama"
    );

    // Aggiungi plugin di routing
    kernelBuilder.Plugins.AddFromType<RoutingPagePlugin>("Routing");

    return kernelBuilder.Build();
});

builder.Services.AddScoped<IUserContextService, UserContextService>();


builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IKnowledgeBaseRetriever, KnowledgeBaseRetriever>();

// builder.Services.AddScoped<IIntentClassificationService, IntentClassificationService>();
// builder.Services.AddScoped<IIntentRouterService, IntentRouterService>();


#region PayPal configuration
builder.Services.AddSingleton<IPayPalClient, PayPalClient>();
builder.Services.Configure<PayPalClientOptions>(options =>
{
    options.ClientId = builder.Configuration.GetRequiredSection("Payments:PayPal")["ClientId"]!;
    options.ClientSecret = builder.Configuration.GetRequiredSection("Payments:PayPal")["ClientSecret"]!;
    options.PayPalUrl = builder.Configuration.GetRequiredSection("Payments:PayPal")["PayPalUrl"]!;
});
#endregion

builder.Services.AddSingleton<TimerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.MapHub<VehicleHub>("/vehicleHub");

app.MapHub<TimerHub>("/timerHub");

app.Run();
