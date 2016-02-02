using CsQuery;

namespace Hemoglobin
{
	public interface IHtmlBinder
	{
		bool HasAccessor(string alias);
		bool HasProcessor(string alias);
		object Access(string alias, string[] arguments, CQ node);
		object Process(string alias, string[] arguments, object value);
		string StructureAccessorAlias { get; }
	}
}
