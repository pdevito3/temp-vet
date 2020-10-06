namespace WebApi.Extensions
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using NSwag;
    using System;

    public static class ServiceExtensions
    {
        #region Swagger Region - Do Not Delete
            public static void AddSwaggerExtension(this IServiceCollection services)
            {
                services.AddSwaggerDocument(config => {
                    config.PostProcess = document =>
                    {
                        document.Info.Version = "v1";
                        document.Info.Title = "MySwaggerDoc";
                        document.Info.Description = "This is my swagger doc";
                        document.Info.Contact = new OpenApiContact
                        {
                            Name = "Paul",
                            Email = "paul@test.com",
                            Url = "https://www.thisispaulswebsite.com",
                        };
                        document.Info.License = new OpenApiLicense()
                        {
                            Name = $"Copyright {DateTime.Now.Year}",
                        };
                    };
                });
            }
        #endregion

        public static void AddApiVersioningExtension(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                // Default API Version
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // use default version when version is not specified
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });
        }

        public static void AddCorsService(this IServiceCollection services, string policyName)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName,
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("X-Pagination"));
            });
        }
    }
}
