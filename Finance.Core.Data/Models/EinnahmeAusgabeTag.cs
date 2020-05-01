using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Data.Models
{
	/// <summary>
	/// Join-Tabelle zwischen EinnahmeAusgabe und Tag
	/// </summary>
	public class EinnahmeAusgabeTag
	{
		public long EinnahmeAusgabeId { get; set; }

		public EinnahmeAusgabe EinnahmeAusgabe { get; set; }

		public long TagId { get; set; }

		public Tag Tag { get; set; }
	}
}
