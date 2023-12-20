using KTX_DAL.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

//Config database
var ConnectStringServer = builder.Configuration.GetValue<string>("DBConnect:ConnectString:Server");
var ConnectStringUID = builder.Configuration.GetValue<string>("DBConnect:ConnectString:Uid");
var ConnectStringPWD = builder.Configuration.GetValue<string>("DBConnect:ConnectString:Pwd");
var ConnectStringDB = builder.Configuration.GetValue<string>("DBConnect:ConnectString:Database");
var connectionString = $"server={ConnectStringServer};database={ConnectStringDB};user={ConnectStringUID};password={ConnectStringPWD};persist security info=False;connect timeout=300";
var serverVersion = new MariaDbServerVersion(new Version(10, 4, 18));
builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.OutputFormatters.RemoveType<StringOutputFormatter>();
    options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
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
