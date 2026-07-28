using DriveAddis.Application.Instructors.Queries;
using DriveAddis.Application.Interfaces;
using DriveAddis.Infrastructure.Persistence;
using DriveAddis.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database connection
builder.Services.AddDbContext<DriveAddisDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DriveAddisDatabase")));

// 2. MediatR — scans the Application project for all handlers (like SearchInstructorsHandler)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SearchInstructorsQuery).Assembly));

// 3. Repository — tells the app "whenever someone asks for IInstructorRepository, give them InstructorRepository"
builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();

// 4. Controllers + OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapControllers();

app.Run();