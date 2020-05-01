using Finance.Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finance.Core.Data.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly FinanceContext _Context;

		public UserRepository(FinanceContext contex)
		{
			this._Context = contex;
		}

		public FinanceAppUser GetFinanceUser(string userName)
		{
			IQueryable<FinanceAppUser> users = null;

			try
			{
				users = this._Context.Users.Where(u => u.Email.Equals(userName, StringComparison.OrdinalIgnoreCase) || u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

				if (users.Count() == 1)
					return users.FirstOrDefault();
			}
			catch (Exception ex)
			{
				//TODO Fehler loggen wenn es zu Probleme beim Holen des Users kamm
				throw ex;
			}

			if (users == null || users.Count() == 0)
				throw new Exception($"Es wurde kein User zu dem Username {userName} gefunden!");

			if (users.Count() > 1)
				throw new Exception($"Es wurde mehr als ein User zu dem Username {userName} gefunden!");

			throw new Exception("Irgendein anderer Fehler ist aufgetreten beim Holen des Users");
		}
	}

	public interface IUserRepository
	{
		FinanceAppUser GetFinanceUser(string userName);
	}
}
