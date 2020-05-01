using AutoMapper;
using Finance.Core.Data.Models;
using Finance.Core.WebApi.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finance.Core.WebApi.Data
{
	public class FinanceMappingProfile : Profile
	{
		public FinanceMappingProfile()
		{
			CreateMap<EinnahmeAusgabe, EinnahmeAusgabeViewModel>()
				.ForMember(e => e.ListTags, a => a.MapFrom(e => e.Tags))
				.ForMember(e => e.BuchungsdatumJson, a => a.MapFrom(e => JsonConvert.SerializeObject(e.Buchungsdatum)))
				.ReverseMap();

			CreateMap<Tag, TagViewModel>()
				.ForMember(o => o.Benennung, ex => ex.MapFrom(o => o.TagBenennung))
				.ReverseMap();

			//Achtung FinanceImage.Path darf nicht nach ImageMetaViewModel.ImageSource gemappt werden, nicht das selbe!!!
			CreateMap<FinanceImage, ImageMetaViewModel>()
				.ForMember(vm => vm.ImageId, ex => ex.MapFrom(m => m.Id))
				.ForMember(vm => vm.ImageSource, ex => ex.MapFrom(m => m.Url))
				.ReverseMap();
		}
	}
}
