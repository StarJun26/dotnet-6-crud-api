﻿using Microsoft.Extensions.Logging.Configuration;
using System.Text.Json.Serialization;
using WebApi.Helpers;
using WebApi.Logger;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// add services to DI container
{
    var services = builder.Services;
    var env = builder.Environment;

    //use in memory DB
    services.AddDbContext<DataContext>();
    /*
        //connect to SQL Server with connection string
        var connectionString = string.Empty;
        services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));
    */

    services.AddCors();
    services.AddControllers().AddJsonOptions(x =>
    {
        // serialize enums as strings in api responses (e.g. Role)
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // ignore omitted parameters on models to enable optional params (e.g. User update)
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // configure DI for application services
    services.AddScoped<IUserService, UserService>();
    services.AddSingleton<ILoggingService, TestableLogger>();

    //configure Swagger/OpenAPI
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

var app = builder.Build();

// configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

{
    // global cors policy
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

    // global error handler
    app.UseMiddleware<ErrorHandlerMiddleware>();

    app.MapControllers();
}

app.Run("http://localhost:4000");