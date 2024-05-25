using Microsoft.EntityFrameworkCore;
using TimeTable;
using TimeTable.Core.DbContext;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .Cors(builder.Configuration.GetValue<string>("App:CorsOrigins", "")!)
    .AddHttpContextAccessor();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwagger("Time Table")
    .AddControllers();
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TimeTableContext>(options => options.UseSqlServer(connection));
var app = builder.Build();
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<TimeTableContext>();
context.Database.Migrate();
app.MapControllers();
app
    .UseCors("Default")
    .UseSwagger()
    .UseSwaggerUI()
    .UseHttpsRedirection();
app.Run();