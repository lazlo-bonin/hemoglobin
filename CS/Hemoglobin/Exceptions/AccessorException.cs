using System;

namespace Hemoglobin
{
	public class AccessorException : HemoglobinException
	{
		public string Alias { get; set; }

		internal AccessorException(string alias) : base() { Alias = alias; }
		internal AccessorException(string alias, string message) : base(message) { Alias = alias; }
		internal AccessorException(string alias, string message, Exception innerException) : base(message, innerException) { Alias = alias; }

		internal static AccessorException UnknownAlias(string alias)
		{
			return new AccessorException(alias, string.Format("Unknown accessor: '{0}'.", alias));
		}

		internal static AccessorException InvalidArgumentCount(string alias)
		{
			return new AccessorException(alias, string.Format("Invalid number of arguments for the '{0}' accessor.", alias));
		}

		internal static ProcessorException InvalidInput(string alias)
		{
			return new ProcessorException(alias, string.Format("Invalid input for the '{0}' accessor.", alias));
		}
	}
}
