using System;

namespace Hemoglobin
{
	public class ProcessorException : HemoglobinException
	{
		public string Alias { get; set; }

		internal ProcessorException(string alias) : base() { Alias = alias; }
		internal ProcessorException(string alias, string message) : base(message) { Alias = alias; }
		internal ProcessorException(string alias, string message, Exception innerException) : base(message, innerException) { Alias = alias; }

		internal static ProcessorException UnknownAlias(string alias)
		{
			return new ProcessorException(alias, string.Format("Unknown processor: '{0}'.", alias));
		}

		internal static ProcessorException InvalidArgumentCount(string alias)
		{
			return new ProcessorException(alias, string.Format("Invalid number of arguments for the '{0}' processor.", alias));
		}

		internal static ProcessorException InvalidInput(string alias)
		{
			return new ProcessorException(alias, string.Format("Invalid input for the '{0}' processor.", alias));
		}

		internal static ProcessorException ProcessingFailed(string alias, string field, Exception innerException)
		{
			throw new ProcessorException(alias, string.Format("Failed to run processor '{0}' on field '{1}'.", alias, field), innerException);
		}
	}
}
