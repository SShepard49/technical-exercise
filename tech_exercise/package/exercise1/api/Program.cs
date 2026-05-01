using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Dapper;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Infrastructure.Data;
using StargateAPI.Infrastructure.Logging;
using StargateAPI.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "AcsUiDev";

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Development-only CORS policy so the Angular dev server (ng serve, default port 4200)
// can call the API from the browser. Production behavior is unchanged.
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Putting the database in the App_Data directory to avoid potential issues with the default location.
var rawConnectionString = builder.Configuration.GetConnectionString("StarbaseApiDatabase")
    ?? throw new InvalidOperationException("StarbaseApiDatabase connection string missing.");
var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder(rawConnectionString);
if (!Path.IsPathRooted(sqliteConnectionStringBuilder.DataSource))
{
    var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    Directory.CreateDirectory(dataDirectory);
    sqliteConnectionStringBuilder.DataSource = Path.Combine(dataDirectory, sqliteConnectionStringBuilder.DataSource);
}

builder.Services.AddDbContext<StargateContext>(options =>
    options.UseSqlite(sqliteConnectionStringBuilder.ToString()));

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.AddRequestPreProcessor<CreatePersonPreProcessor>();
    cfg.AddRequestPreProcessor<UpdatePersonByNamePreProcessor>(); 
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});
builder.Services.AddScoped<ExceptionHandlingMiddleware>();
builder.Services.AddScoped<IProcessLogWriter, ProcessLogWriter>();

var app = builder.Build();

// Changed the DateTiem to DateOnly to better represent the data and avoid confusion about timezones.
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(DevCorsPolicy);
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

await StargateSeeder.SeedAsync(app.Services);

app.Run();

public partial class Program { } // Needed for testing