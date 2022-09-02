using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nexer.CSV.Facade;
using Nexer.Domain.Facades;
using Nexer.Domain.Interfaces.CSVFacade;
using Nexer.Domain.Interfaces.Notificator;
using Nexer.Domain.Interfaces.Services;
using Nexer.Domain.Interfaces.ZipFacade;
using Nexer.Domain.Models.Configurations;
using Nexer.Domain.Models.ValidationModels;
using Nexer.Domain.Notificator;
using Nexer.Domain.Repositories;
using Nexer.Domain.Services;
using Nexer.Domain.Validations;
using Nexer.Infrastructure.Context;
using Nexer.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.SwaggerGen;
using static Nexer.WeatherAPI.Configurations.SwaggerConfiguration;

namespace Nexer.WeatherAPI.Configurations
{
    public static class DependencyInjectionConfiguration
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            var azureBlobStorageSection = configuration.GetSection("AzureBlobStorage");

            //Contexts
            services.AddScoped<IAzureBlobContext>(x => new AzureBlobContext(
                azureBlobStorageSection.GetValue<string>("ConnectionString"),
                azureBlobStorageSection.GetValue<string>("ContainerName")));

            //Repositories
            services.AddScoped<IWeatherStorageRepository, WeatherStorageRepository>();

            //Services
            services.AddScoped<IWeatherServices, WeatherServices>();

            //Configurations
            services.AddScoped<INotificator, Notificator>();
            services.AddScoped<ICsvFacade, CsvFacade>();
            services.AddScoped<IZipFacade, ZipFacade>();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddScoped<IValidator<GetDataValidationModel>, GetDataValidation>();

            //Options
            services.AddOptions<WeatherConfiguration>("WeatherConfiguration");

            return services;
        }
    }
}
