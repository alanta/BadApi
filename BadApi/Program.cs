using BadApi.Account;
using BadApi.BrokenAccessControl;
using BadApi.Data;
using BadApi.DDoS;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { });

builder.SetupAuthorization();

/*builder.Services.AddControllers()
    .AddXmlSerializerFormatters(); // Add XML serializer formatters*/

builder.ConfigureRateLimiting("fixed");

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapPost("/xxe/bad", (Delegate)BadApi.XXE.Endpoints.Bad);
app.MapPost("/xxe/good", (Delegate)BadApi.XXE.Endpoints.Good);

app.MapPost("/sqlinjection/bad", (Delegate)BadApi.SqlInjection.Endpoints.Bad);
app.MapPost("/sqlinjection/good", (Delegate)BadApi.SqlInjection.Endpoints.Good);


BadApi.Account.Endpoints.Register(app);
BadApi.BrokenAccessControl.Endpoints.Register(app);

//app.MapPost("/user/{id}/changepassword", (Delegate)BadApi.BrokenAccessControl.Endpoints.ChangePassword)
//    .RequireAuthorization("BasicAuthentication");

app.MapPost("/ddos/bad", (Delegate)BadApi.DDoS.Endpoints.Bad);
app.MapPost("/ddos/better", (Delegate)BadApi.DDoS.Endpoints.Better);
app.MapPost("/ddos/good", (Delegate)BadApi.DDoS.Endpoints.Good).RequireRateLimiting("fixed");

DatabaseUtils.Seed();
app.Run();
