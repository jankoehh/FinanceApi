using Finance.Core.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Data
{
	public class FinanceContext : IdentityDbContext<FinanceAppUser>
	{
		public FinanceContext(DbContextOptions<FinanceContext> options) : base(options)
		{
		}

		public DbSet<Anhang> Anhaenge { get; set; }
		public DbSet<Einstellung> Einstellungen { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<Waehrung> Waehrungen { get; set; }
		public DbSet<EinnahmeAusgabe> EinnahmenAusgaben { get; set; }
		public DbSet<EinnahmeAusgabeTag> EinnahmeAusgabeTags { get; set; }
		public DbSet<FinanceImage> EinnahmeAusgabeBilder { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<EinnahmeAusgabeTag>()
				.HasKey(eat => new { eat.EinnahmeAusgabeId, eat.TagId });
			builder.Entity<EinnahmeAusgabeTag>()
				.HasOne(eat => eat.EinnahmeAusgabe)
				.WithMany(ea => ea.EinnahmenAusgabenTags)
				.HasForeignKey(eat => eat.EinnahmeAusgabeId);
			builder.Entity<EinnahmeAusgabeTag>()
				.HasOne(eat => eat.Tag)
				.WithMany(t => t.EinnahmenAusgabenTags)
				.HasForeignKey(eat => eat.TagId);
		}
	}
}
