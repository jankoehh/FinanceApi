using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Finance.Core.WebApi.ViewModels
{
	public class TagViewModel
	{
		public long Id { get; set; }

		[Required]
		[MinLength(3)]
		public string Benennung { get; set; }
	}
}
