using BadApi.Account;
using BadApi.BrokenAccessControl;
using BadApi.Data;
using BadApi.DDoS;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
BadApi.SqlInjection.Endpoints.Setup(builder);

var db = new Database();
builder.Services.AddSingleton(db);

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { });

builder.SetupAuthorization();
builder.ConfigureRateLimiting("fixed");

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

BadApi.XXE.Endpoints.Register(app);
BadApi.SqlInjection.Endpoints.Register(app);
BadApi.OverPosting.Endpoints.Register(app);
BadApi.Account.Endpoints.Register(app);
BadApi.BrokenAccessControl.Endpoints.Register(app);
BadApi.DDoS.Endpoints.Register(app);

db.Seed();
app.Run();

public partial class Program { }