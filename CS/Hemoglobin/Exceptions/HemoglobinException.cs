using System;

namespace Hemoglobin
{
	public class HemoglobinException : Exception
	{
		internal HemoglobinException() : base() { }
		internal HemoglobinException(string message) : base(message) { }
		internal HemoglobinException(string message, Exception innerException) : base(message, innerException) { }
	}
}
