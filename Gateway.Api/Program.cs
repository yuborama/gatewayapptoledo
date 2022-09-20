using Auth.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Listado de Microservicios
var PetShop = "pet_shop";

builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
        ValidateIssuer = true,
        ValidAudience = builder.Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("TokenSettings").GetValue<string>("Key"))),
        ValidateIssuerSigningKey = true
    };
});

// Cargango Microservicios
builder.Services.AddHttpContextAccessor();
builder.Services.AddGraphQLServer().AddType<UploadType>();
builder.Services.AddHeaderPropagation(o =>
{
    o.Headers.Add("Authorization");
});

var environment = Environment.GetEnvironmentVariable("APP_ENV");

builder.Services.AddHttpClient(PetShop, c => c.BaseAddress = new Uri("http://localhost:7001/graphql/")).AddHeaderPropagation();

builder.Services.AddAuthorization();

// Carga de Esquemas individuales de Microservicios
builder.Services.AddGraphQLServer()
    .AddType<UploadType>()
    .AddRemoteSchema(PetShop);

builder.Services.AddCors(option => {
    option.AddPolicy("allowedOrigin",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        );
});

var app = builder.Build();
app.UseCors("allowedOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.UseHeaderPropagation();
app.UseWebSockets();
app.MapGraphQL();

app.MapGet("/", () => "");

//KUBERNETES
//liveness, readiness and startup probes for containers
app.MapGet("/liveness", () => "Liveness Mailing");
app.MapGet("/readiness", () => "Readiness Mailing");

app.Run();