using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Finance.Core.Data.Models
{
	public class Einstellung
	{
		[Key]
		public long Id { get; set; }

		public Waehrung HauptWaehrung { get; set; }

		public string StandartpfadAnhaenge { get; set; }
	}
}
