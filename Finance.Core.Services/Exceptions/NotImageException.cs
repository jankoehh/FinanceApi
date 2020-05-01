using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Core.Services.Exceptions
{
	public class NotImageException : Exception
	{
		public NotImageException()
		{
		}

		public NotImageException(string message)
			: base(message)
		{
		}

		public NotImageException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
