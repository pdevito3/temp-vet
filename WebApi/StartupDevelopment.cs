namespace WebApi
{
    using Application;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Persistence;
    using Infrastructure.Shared;
    using Infrastructure.Persistence.Seeders;
    using Infrastructure.Persistence.Contexts;
    using WebApi.Extensions;
    using Infrastructure.Identity;
    using Infrastructure.Identity.Entities;
    using Microsoft.AspNetCore.Identity;
    using Infrastructure.Identity.Seeders;
    using WebApi.Services;
    using Application.Interfaces;

    public class StartupDevelopment
    {
        public IConfiguration _config { get; }
        public StartupDevelopment(IConfiguration configuration)
        {
            _config = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorsService("MyCorsPolicy");
            services.AddApplicationLayer();
            services.AddIdentityInfrastructure(_config);
            services.AddPersistenceInfrastructure(_config);
            services.AddSharedInfrastructure(_config);
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddApiVersioningExtension();
            services.AddHealthChecks();
            services.AddSingleton<ICurrentUserService, CurrentUserService>();

            #region Dynamic Services
            services.AddSwaggerExtension();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            #region Entity Context Region - Do Not Delete

                using (var context = app.ApplicationServices.GetService<VetClinicDbContext>())
                {
                    context.Database.EnsureCreated();

                    #region VetClinicDbContext Seeder Region - Do Not Delete
                    
                    PetSeeder.SeedSamplePetData(app.ApplicationServices.GetService<VetClinicDbContext>());
                    VetSeeder.SeedSampleVetData(app.ApplicationServices.GetService<VetClinicDbContext>());
                    CitySeeder.SeedSampleCityData(app.ApplicationServices.GetService<VetClinicDbContext>());
                    #endregion
                }

            #endregion

            #region Identity Context Region - Do Not Delete

            var userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
            var roleManager = app.ApplicationServices.GetService<RoleManager<IdentityRole>>();
            RoleSeeder.SeedDemoRolesAsync(roleManager);

            // user seeders -- do not delete this comment
            pdevitoSeeder.SeedUserAsync(userManager);

            #endregion

            app.UseCors("MyCorsPolicy");

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseErrorHandlingMiddleware();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/api/health");
                endpoints.MapControllers();
            });

            #region Dynamic App
            app.UseSwaggerExtension();
            #endregion
        }
    }
}
