﻿using System.Text;
using CarSharingApp.CarService.Application.CommandHandlers.CarCommandHandlers;
using CarSharingApp.CarService.Application.CommandHandlers.CarStateHandlers;
using CarSharingApp.CarService.Application.CommandHandlers.CommentCommandHandlers;
using CarSharingApp.CarService.Application.CommandHandlers.ImageCommandHandlers;
using CarSharingApp.CarService.Application.Commands.CarCommands;
using CarSharingApp.CarService.Application.Commands.CarStateCommands;
using CarSharingApp.CarService.Application.Commands.CommentCommands;
using CarSharingApp.CarService.Application.Commands.ImageCommands;
using CarSharingApp.CarService.Application.DTO_s.CarState;
using CarSharingApp.CarService.Application.Mapping;
using CarSharingApp.CarService.Application.Queries.CarQueries;
using CarSharingApp.CarService.Application.Queries.CommentQueries;
using CarSharingApp.CarService.Application.Queries.ImageQueries;
using CarSharingApp.CarService.Application.QueryHandlers.CarQueryHandlers;
using CarSharingApp.CarService.Application.QueryHandlers.CommentQueryHandlers;
using CarSharingApp.CarService.Application.QueryHandlers.ImageQueryHandlers;
using CarSharingApp.CarService.Application.Repositories;
using CarSharingApp.CarService.Application.Responses.Car;
using CarSharingApp.CarService.Application.Responses.Comment;
using CarSharingApp.CarService.Application.Responses.Image;
using CarSharingApp.CarService.Infrastructure.DataBase;
using CarSharingApp.CarService.Infrastructure.Repositories;
using CarSharingApp.CarService.WebAPI.Extensions;
using CarSharingApp.CarService.WebAPI.MiddleWares;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Minio;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarSharingApp.CarService.WebAPI;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        
        services.AddTransient<IRequestHandler<CreateCarCommand, CarResponse>, CreateCarHandler>();
        services.AddTransient<IRequestHandler<UpdateCarCommand, CarResponse>, UpdateCarHandler>();
        services.AddTransient<IRequestHandler<DeleteCarCommand, CarResponse>, DeleteCarHandler>();
        
        services.AddTransient<IRequestHandler<UpdateCommentCommand, CommentResponse>, UpdateCommentHandler>();
        services.AddTransient<IRequestHandler<CreateCommentCommand, CommentResponse>, CreateCommentHandler>();
        services.AddTransient<IRequestHandler<DeleteCommentCommand, CommentResponse>, DeleteCommentHandler>();
        
        services.AddTransient<IRequestHandler<CreateImageCommand, ImageCommandResponse>, CreateImageHandler>();
        services.AddTransient<IRequestHandler<UpdateImagePriorityCommand, ImageCommandResponse>, UpdateImagePriorityHandler>();
        services.AddTransient<IRequestHandler<DeleteImageCommand, ImageCommandResponse>, DeleteImageHandler>();

        services.AddTransient<IRequestHandler<UpdateCarActivityCommand, CarStateDto>, UpdateCarActivityHandler>();
        services.AddTransient<IRequestHandler<UpdateCarLocationCommand, CarStateDto>, UpdateCarLocationHandler>();

        services.AddTransient<IRequestHandler<GetCarQuery, CarFullResponse>, GetCarHandler>();
        services.AddTransient<IRequestHandler<GetCarsByParamsQuery, IEnumerable<CarResponse>>, GetCarsByParamsHandler>();
        
        services.AddTransient<IRequestHandler<GetCommentsByCarQuery, IEnumerable<CommentResponse>>, GetCommentsByCarHandler>();
        
        services.AddTransient<IRequestHandler<GetImagesByCarQuery, IEnumerable<ImageQueryResponse>>, GetImagesByCarHandler>();
    }
    
    public static async Task InitializeMinio(IServiceCollection services, ConfigurationManager config)
    {
        var endPoint = config["MinIO-Settings:EndPoint"];
        var accessKey = config["MinIO-Settings:AccessKey"];
        var secretKey = config["MinIO-Settings:SecretKey"];
        
        services.AddMinio(configureClient => configureClient
            .WithEndpoint(endPoint)
            .WithCredentials(accessKey, secretKey));
    }
    
    public static void ConfigureRepository(IServiceCollection services)
    {
        services.AddTransient<IMinioRepository, MinioRepository>();
        services.AddTransient<ICarImageRepository, CarImageRepository>();
        services.AddTransient<ICarRepository, CarRepository>();
        services.AddTransient<ICommentRepository, CommentRepository>();
        services.AddTransient<ICarStateRepository, CarStateRepository>();
    }
    
    public static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigurationExtension>();
    }
    
    public static void ConfigureDataBase(IServiceCollection services, ConfigurationManager config)
    {
        var connectionString = config.GetConnectionString("DataBase");
        services.AddDbContext<CarsContext>(options =>
            options.UseSqlServer(connectionString));
    }
    
    public static void ConfigureMiddlewares(WebApplication app)
    {
        app.UseMiddleware<ExceptionAndLoggingMiddleware>();
    }
    
    public static void ConfigureAuth(IServiceCollection services, ConfigurationManager config)
    {
        services.AddAuthentication(x =>
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidAudience = config["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
    }
}