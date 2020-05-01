using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Finance.Core.Data;
using Finance.Core.Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Finance.Core.WebApi
{
	public class Startup
	{
		private readonly IConfiguration _Configuration;
		private readonly IHostingEnvironment _CurrentEnvironment;

		public Startup(IConfiguration configuration, IHostingEnvironment env)
		{
			_Configuration = configuration;
			_CurrentEnvironment = env;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
			services.AddIdentity<FinanceAppUser, IdentityRole>(config =>
			{
				config.User.RequireUniqueEmail = true;
				config.Password.RequiredLength = 10;
			})
			 .AddEntityFrameworkStores<FinanceContext>();

			//Cookie und JwtBaerer-Token Authentication hinzufügen
			services.AddAuthentication()
				.AddCookie()
				.AddJwtBearer(cfg =>
				{
					cfg.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidIssuer = _Configuration["Tokens:Issuer"],
						ValidAudience = _Configuration["Tokens:Audience"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuration["Tokens:Key"]))
					};
				});

			//Setzen des ConnectionStrings zur Datenbank in Abhängigkeit ob Entwicklung (Lokal) oder Produktion (Azure)
			services.AddDbContext<FinanceContext>(x => { x.UseSqlServer(_Configuration.GetConnectionString("FinanceConnectionString")); });

			//if (_CurrentEnvironment.IsDevelopment())
			//	services.AddDbContext<FinanceContext>(x => { x.UseSqlServer(_Configuration.GetConnectionString("FinanceConnectionString")); });
			//else
			//	services.AddDbContext<FinanceContext>(x => { x.UseSqlServer(_Configuration.GetConnectionString("FinanceConnectionStringAzure")); });

			//seit Asp Core 3 typeof(Startup) als Parameter übergeben
			services.AddAutoMapper(typeof(Startup));

			//Mappen der Services
			DIMapping.MapServices(services);
		}



		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseAuthentication();
			app.UseCookiePolicy();

			app.UseMvc();

			//app.UseMvc(routes =>
			//{
			//	routes.MapRoute(
			//		name: "default",
			//		template: "{controller=Home}/{action=Index}/{id?}");
			//});
		}
	}
}
