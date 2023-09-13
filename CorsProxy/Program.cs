using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Development.json", true);
else
    builder.Configuration.AddJsonFile("appsettings.json", true);

builder.Services.AddCors();
builder.Services.AddHttpForwarder();


var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

var config = app.Configuration.GetSection("CorsProxy");
var destinationUrl = config.GetValue<string>("DestinationUrl")!;

app.MapForwarder(
    "{**catch-all}",
    destinationUrl, 
    new ForwarderRequestConfig
    {
        ActivityTimeout = TimeSpan.FromSeconds(100)
    }, HttpTransformer.Default);

app.Run();