using AutoMapper;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SpaFramework.App;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.Web.Filters;
using SpaFramework.Web.Filters.SchemaFilters;
using SpaFramework.Web.Hubs;
using SpaFramework.Web.Middleware;
using SpaFramework.Web.Middleware.Exceptions;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VueCliMiddleware;

namespace SpaFramework.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //// Add the database context
            //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default"), x => x.UseNodaTime()));

            //// Add Identity
            //services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            //{
            //    options.Password.RequireDigit = false;
            //    options.Password.RequireLowercase = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequiredLength = 8;
            //})
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

            services.AddDataServices(Configuration);

            // ===== Add Jwt Authentication ========
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null)
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidAudience = Configuration["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                    cfg.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // This was originally here for SignalR hubs, but we use it in other places as well (like viewing action item attachments)
                            if (context.Request.Query.ContainsKey("access_token"))
                            {
                                var accessToken = context.Request.Query["access_token"];
                                context.Token = accessToken;
                            }

                            //var path = context.HttpContext.Request.Path;

                            //// If this is a SignalR hub, authenticate the connection here
                            //// https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-5.0
                            //if (!string.IsNullOrEmpty(path) && path.HasValue && path.Value.EndsWith("-hub"))
                            //{
                            //    var accessToken = context.Request.Query["access_token"];
                            //    context.Token = accessToken;
                            //}

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddCors(options =>
            {
                options.AddPolicy("PublicFormPolicy", builder =>
                {
                    builder
                        .WithOrigins(Configuration.GetValue<string>("CorsPolicy:PublicForms").Split(","))
                        .AllowAnyHeader();
                });
            });
                
            // This allows us to use both the JWT and "Basic" authentication schemes
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, "Basic")
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // Add health checks
            services
                .AddHealthChecks()
                .AddSqlServer(Configuration.GetConnectionString("Default"), name: "SQL Database", tags: new string[] { /*"Azure"*/ });
                //.AddAzureQueueStorage(Configuration["AzureWebJobsStorage"], name: "Azure Queue Storage", tags: new string[] { "Azure" });

            services
                .AddHealthChecksUI(setup =>
                {
                    setup.AddHealthCheckEndpoint("Core App", "/api/health");
                })
                .AddSqlServerStorage(Configuration.GetConnectionString("Default"));
            //.AddInMemoryStorage();

            // Add MVC
            services
                .AddMvc(options =>
                {
                    // Use camelCase in URLs and routing
                    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Formatting = Formatting.Indented;

                    // Use camelCase on DTOs
                    //options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

                });
            //.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services
                .AddSignalR(x =>
                {
                    //x.EnableDetailedErrors = true;
                })
                //.AddAzureSignalR()
                .AddNewtonsoftJsonProtocol(options =>
                {
                    options.PayloadSerializerSettings.Formatting = Formatting.Indented;

                    options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.PayloadSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

                });

            // Add Swagger (via Swashbuckle)
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SPA Framework API",
                    Version = "v1",
                    Description = @"
## Authentication

Most of these API endpoints are only available to authenticated users. To authenticate, make a call to `GET /api/account/login`. Assuming valid credentials are provided, a JWT token will be
returned. This token should be included in the `Authorization` header as a bearer token on subsequent requests.

Basic HTTP Authentication is also supported, using the standard pattern of `username:password`.

Within Swagger UI, you can click the 'Authorize' button to use either a JWT token or basic authentication on requests.

## Including Related Data

The `includes` parameter on many of these calls lets you specify related objects to include in the results. The parameter is a comma-delimited list of related objects; the objects use dotted
notation. For instance, to include `schedule` items when calling `/api/donors`, set `includes` to `schedules`. If you also wanted to include `itemTypes`, which is a property of `schedule`,
you'd set `includes` to `schedule,schedule.itemTypes`.

## Filtering Results

Results can be filtered with the `filter` parameter. You can use basic conditional operations, parentheses for grouping, null checks, and operators like AND and OR. Here are some examples
(using the `/api/donor` endpoint):
* `city=""PORTLAND""`
* `city=""Portland"" or state=""OR""`
* `phoneNumber1 != null`
* `phoneNumber1 = null`
* `(city=""Portland"" or state=""OR"") and phoneNumber1 != null`
* `id = 10000`

There are also a handful of functions you can use, including `StartsWith`, `EndsWith`, and `Contains`. Note that, especially across large datasets or non-indexed columns, these can perform poorly.
* `city.Contains(""Port"")`
* `city.Contains(""Port"") and !city.EndsWith(""land"")`

Note that not all fields can be filtered.

By default, filter values follow the database collation, so they're case insensitive.

Double quotes should be used around string literals.

Some lambda expressions can also be used. For instance, if there's a many-to-many relationship from `post` to `author` (where `post` has an `authors` navigation property), a filter 
like this against `post` would return all posts by the author John Doe:
`authors.Any(x => x.FirstName = ""John"" and x.LastName = ""Doe"")`

## Ordering Results

Results can be ordered with the `order` parameter. This is a comma-separted list of fields to sort on. Each field can have an optional `asc` or `desc` afterwards to indicate direction. Here
are some examples:

* `lastName`
* `lastName desc`
* `lastName desc, firstName asc`

Note that not all fields can be sorted.

## Limit, Offset, and Total Count

When return a list of items, the `limit` parameter sets the maximum number of items that will be returned. The `offset` parameter specifies the index of the first item to be returned; this defaults
to 0. Together, these parameters can be used for paging.

Calls to return a list of items also include an `X-Total-Count` header which returns the total number of items available (which could be used to determine the total number of pages, for instance).
However, to improve performance, this won't return the full count, only enough to indicate if there are additional pages. A `maxCount` parameter can be provided which specifies what the highest
number is that should be returned in `X-Total-Count`. By default, this value is determined automatially based on the `limit` and `offset`, and is returned in the `X-Total-Count-Max` header. In other
words, if `X-Total-Count` equals `X-Total-Count-Max`, there are probably more results available than the number returned by `X-Total-Count`.
"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Call /api/account/login to get a JWT token.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
                {
                    Description = "Basic HTTP Authentication.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Basic"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            },
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Basic",
                                Type = ReferenceType.SecurityScheme
                            },
                            Name = "Basic",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                c.CustomSchemaIds(type => type.ToString());

                c.OperationFilter<ResponseHeaderOperationFilter>();

                //c.DocumentFilter<DtoDocumentFilter>();
                c.SchemaFilter<DtoMappingSchemaFilter>();
                c.SchemaFilter<GenericEntitySchemaFilter>();

                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "SpaFramework.Web.xml"));
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "SpaFramework.Core.xml"));
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "SpaFramework.App.xml"));
            });
            services.AddSwaggerGenNewtonsoftSupport();

            // In production, the VueJS files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddAppServices();

            services.AddHostedService<StartupService>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            dbContext.Database.Migrate();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "SPA Framework API - V1");
                c.DefaultModelExpandDepth(3);
                c.DefaultModelsExpandDepth(3);
            });

            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            // This must be before UseMVC
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/api/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI();

                endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");

                endpoints.MapHub<NotificationHub>("/notification-hub");

                if (env.IsDevelopment())
                {
                    // NOTE: VueCliProxy is meant for developement and hot module reload
                    // NOTE: SSR has not been tested
                    // Production systems should only need the UseSpaStaticFiles() (above)
                    // You could wrap this proxy in either
                    // if (System.Diagnostics.Debugger.IsAttached)
                    // or a preprocessor such as #if DEBUG
                    endpoints.MapToVueCliProxy(
                        "{*path}",
                        new SpaOptions { SourcePath = "ClientApp" },
                        //npmScript: (System.Diagnostics.Debugger.IsAttached) ? "serve" : null,
                        regex: "App running",
                        forceKill: true
                    );
                }
            });
        }
    }

    /// <summary>
    /// We want to use camelCased route names. There's a built-in option to use lowercase URLs, but we want camel casing.
    /// 
    /// Based on code from https://stackoverflow.com/questions/40334515/automatically-generate-lowercase-dashed-routes-in-asp-net-core
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string TransformOutbound(object value)
        {
            if (value == null)
                return null;

            // Slugify value
            //return Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
            string v = value.ToString();
            return v[0].ToString().ToLower() + v.Substring(1);
        }
    }
}
