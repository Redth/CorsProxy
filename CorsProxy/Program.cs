using System.Diagnostics;
using System.Net;
using CorsProxy;
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

var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
{
    UseProxy = false,
    AllowAutoRedirect = true,
    AutomaticDecompression = DecompressionMethods.None,
    UseCookies = false,
    ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
    ConnectTimeout = TimeSpan.FromSeconds(15),
    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
    {
        RemoteCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback((sender, cert, chain, err) =>
        {
            return true;
        })
    }
}) ;

app.MapForwarder(
    "{**catch-all}",
    destinationUrl,
    new ForwarderRequestConfig
    {
        ActivityTimeout = TimeSpan.FromSeconds(100),
        Version = new Version(1, 1)
    },
    HttpTransformer.Default);
    //new CustomTransformer(app.Configuration),
    //httpClient);

app.Run();