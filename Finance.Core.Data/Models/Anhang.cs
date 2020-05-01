using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Finance.Core.Data.Models
{
	public class Anhang
	{
		[Key]
		public long Id { get; set; }

		public string Benennung { get; set; }

		public string Pfad { get; set; }
	}
}
