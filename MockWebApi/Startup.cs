using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MockWebApi.ActionFilters;
using MockWebApi.Log;
using MockWebApi.Policy;
using System.Text;

namespace MockWebApi
{
    public class Startup
    {
        public const string DefaultPolicy = "DefaultPolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<RequestLogAttribute>();
            services.AddScoped<CustomAuthorizationAttribute>();

            // Replace the default authorization policy provider with our own
            // custom provider which can return authorization policies for given
            // policy names (instead of using the default policy provider)
            services.AddSingleton<IAuthorizationPolicyProvider, RoleRequiredPolicyProvider>();

            // As always, handlers must be provided for the requirements of the authorization policies
            services.AddSingleton<IAuthorizationHandler, RoleRequiredAuthorizationHandler>();
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                        {
                            ValidAudience = "http://filesysidprovider.eggs.com",
                            ValidIssuer = Configuration["Auth:SlpToken:Issuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF32.GetBytes(Configuration["Auth:SlpToken:SigningKey"])),
                            RequireExpirationTime = true,
                            RequireAudience = true,
                            ValidateIssuer = true,
                            ValidateAudience = true
                        };
                });

            services.AddControllers();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(DefaultPolicy, policy =>
                {
                    policy.AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme
                    );
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("iss");
                    policy.RequireClaim("aud");
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

            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    
}
