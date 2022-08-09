using BareboneServicePolicy.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace BareboneServicePolicy
{
    public class Startup
    {
        public const string DefaultPolicy = "MyPolicy";
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddCors(o =>
            {
                o.AddPolicy(DefaultPolicy, builder => builder
                    .SetIsOriginAllowed(hostName => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("X-Error-Correlation-Id")
                    );

                o.DefaultPolicyName = DefaultPolicy;
            });

            services.AddHttpContextAccessor();
            services.AddTransient<JwtUserPrincipalUserFilter>();

            // Add the authentication and authorization services for the desired authentication scheme
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(cfg => cfg.SlidingExpiration = true);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidAudience = Configuration["Auth:SlpToken:Audience"],
                        ValidIssuer = Configuration["Auth:SlpToken:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Auth:SlpToken:SigningKey"])),
                    };

                    cfg.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            await PrincipalConformity.ConformToken(context);
                        }
                    };
                });

            services.AddControllers();

            // following is an alternative to principal conformity (create fake user)
            services.AddTransient<JwtUserPrincipalUserFilter>();
            services.AddMvcCore(options =>
            {
                options.Filters.AddService<JwtUserPrincipalUserFilter>();
            });

            services.AddAuthorization(authzOptions =>
            {
                // Define the policy here
                authzOptions.AddPolicy("HasProtectedAccess", policyConfig =>
                {
                    policyConfig.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    // Add requirements to satisfy this policy
                    policyConfig.RequireAuthenticatedUser();
                    //policyConfig.RequireClaim("Scope", "myapi:protected-access");
                });
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        
    }
}
