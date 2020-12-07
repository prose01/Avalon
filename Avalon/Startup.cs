using Avalon.Data;
using Avalon.Helpers;
using Avalon.Interfaces;
using Avalon.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Avalon
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add service and create Policy with options
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins("http://localhost:4200")
                                .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "HEAD")
                                .AllowAnyHeader()
                                .AllowCredentials()
                    );
            });

            // Add framework services.
            services.AddMvc().AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                // TODO: https://www.ryadel.com/en/jsonserializationexception-self-referencing-loop-detected-error-fix-entity-framework-asp-net-core/
                //options.JsonSerializerOptions.ReferenceHandling = ReferenceHandling.Preserve; 
            });

            // Add authentication.
            string domain = $"https://{Configuration["Auth0:Domain"]}/";
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = Configuration["Auth0:ApiIdentifier"];
            });

            // register the scope authorization handler
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            // Add our repository type(s)
            services.AddSingleton<ICurrentUserRepository, CurrentUserRepository>();
            services.AddSingleton<IProfilesQueryRepository, ProfilesQueryRepository>();

            // Add our helper method(s)
            services.AddSingleton<IHelperMethods, HelperMethods>();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Avalon API",
                    Description = "A simple example Avalon API",
                    //TermsOfService = "None",
                    //Contact = new Contact { Name = "Peter Rose", Email = "", Url = "http://Avalon.com/" },
                    //License = new Swashbuckle.AspNetCore.Swagger.License { Name = "Use under LICX", Url = "http://Avalon.com" }
                });
                
                // Define the ApiKey scheme that's in use (i.e. Implicit Flow)
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("/auth-server/connect/authorize", UriKind.Relative),
                            Scopes = new Dictionary<string, string>
                            {
                                { "readAccess", "Access read operations" },
                                { "writeAccess", "Access write operations" }
                            }
                        }
                    }
                });
            });

            services.Configure<Settings>(options =>
            {
                options.ConnectionString = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                options.Database = Configuration.GetSection("MongoConnection:Database").Value;
                options.Auth0Id = Configuration.GetSection("Auth0:Claims-nameidentifier").Value;
                options.Auth0ApiIdentifier = Configuration.GetSection("Auth0:ApiIdentifier").Value;
                options.Auth0TokenAddress = Configuration.GetSection("Auth0:TokenAddress").Value;
            });

            //TODO: Out maybe?
            //services.AddTransient<ICurrentUserRepository, CurrentUserRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Enable routing
            app.UseRouting();

            if (env.IsDevelopment())
            {
                //app.UseStatusCodePages();     https://stackoverflow.com/questions/51719195/asp-net-core-useexceptionhandler-not-working-post-request
                //app.UseDatabaseErrorPage();   https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-3.1
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/error-local-development");
            }
            //else
            //{
            //    app.UseExceptionHandler("/error");
            //}

            // Shows UseCors with CorsPolicyBuilder.
            // Remember to remove Cors for production.
            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            // Enable Authentication
            app.UseAuthentication();

            // Enable Authorization
            app.UseAuthorization();

            // Add endpoints 
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Avalon V1");
            });
        }
    }
}
