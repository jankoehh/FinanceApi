using Finance.Core.Data.Repositories;
using Finance.Core.Services.FileServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finance.Core.WebApi
{
	public class DIMapping
	{
		public static void MapServices(IServiceCollection services)
		{
			#region Repositories
			services.AddScoped<IEinnahmenAusgabenRepository, EinnahmenAusgabenRepository>();
			services.AddScoped<ITagsRepository, TagsRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IImageRepository, ImageRepository>();
			#endregion

			#region Services
			services.AddScoped<ICheckDateiGleichBildService, CheckDateiGleichBildService>();
			services.AddScoped<ICreateZufallsDateinamenService, CreateZufallsDateinamenService>();
			#endregion
		}
	}
}
