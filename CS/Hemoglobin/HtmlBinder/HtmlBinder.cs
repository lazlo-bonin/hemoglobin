using System.Collections.Generic;
using CsQuery;

namespace Hemoglobin
{
	public delegate object Accessor(CQ node, string[] arguments, string alias);
	public delegate object Processor(object value, string[] arguments, string alias);

	public class HtmlBinder : IHtmlBinder
	{
		public Dictionary<string, Accessor> Accessors { get; protected set; }
		public Dictionary<string, Processor> Processors { get; protected set; }
		public string StructureAccessorAlias { get; protected set; }

		public HtmlBinder()
		{
			Accessors = new Dictionary<string, Accessor>();
			Processors = new Dictionary<string, Processor>();
		}

		public bool HasAccessor(string alias)
		{
			return Accessors.ContainsKey(alias);
		}

		public bool HasProcessor(string alias)
		{
			return Processors.ContainsKey(alias);
		}

		public object Access(string alias, string[] arguments, CQ node)
		{
			return Accessors[alias](node, arguments ?? new string[0], alias);
		}

		public object Process(string alias, string[] arguments, object value)
		{
			return Processors[alias](value, arguments ?? new string[0], alias);
		}
	}
}
