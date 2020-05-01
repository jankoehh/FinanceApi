using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finance.Core.Services.FileServices
{
	public class CreateZufallsDateinamenService : ICreateZufallsDateinamenService
	{
		private Random random = new Random();

		public string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
		}

		/// <summary>
		/// Erzeugt einen Zufalls-Dateiname (mit Dateiendung)
		/// </summary>
		/// <param name="dateiEndung"></param>
		/// <returns></returns>
		public string CreateZufallsDateinamen(string dateiEndung)
		{
			try
			{
				return $"{RandomString(30)}.{dateiEndung}";
			}
			catch (Exception ex)
			{
				//TODO Fehler loggen
				throw ex;
			}
		}
	}

	public interface ICreateZufallsDateinamenService
	{
		/// <summary>
		/// Erzeugt einen Zufalls-Dateiname (mit Dateiendung)
		/// </summary>
		/// <param name="dateiEndung"></param>
		/// <returns></returns>
		string CreateZufallsDateinamen(string dateiEndung);
	}
}
