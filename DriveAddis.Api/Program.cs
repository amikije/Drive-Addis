using DriveAddis.Application.Instructors.Queries;
using DriveAddis.Application.Interfaces;
using DriveAddis.Infrastructure.Persistence;
using DriveAddis.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

// 1. Database connection
builder.Services.AddDbContext<DriveAddisDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DriveAddisDatabase")));

// 2. MediatR — scans the Application project for all handlers (like SearchInstructorsHandler)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SearchInstructorsQuery).Assembly));

// 3. Repository — tells the app "whenever someone asks for IInstructorRepository, give them InstructorRepository"
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
// 4. Controllers + OpenAPI
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

var app = builder.Build();

// 5. Apply migrations + seed fake data on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DriveAddisDbContext>();
    context.Database.Migrate();
    DatabaseSeeder.Seed(context);
}

// 6. Middleware pipeline
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

app.Run();