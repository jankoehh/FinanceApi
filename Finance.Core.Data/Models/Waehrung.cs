using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Finance.Core.Data.Models
{
	public class Waehrung
	{
		[Key]
		public long Id { get; set; }

		/// <summary>
		/// bsp. EUR = Euro, USD = US Dollar
		/// </summary>
		public string Abkuerzung { get; set; }

		public string Benennung { get; set; }

		public char Zeichen { get; set; }
	}
}
