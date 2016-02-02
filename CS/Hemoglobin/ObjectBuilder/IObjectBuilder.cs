using System.Collections.Generic;

namespace Hemoglobin
{
	public interface IObjectBuilder
	{
		bool CanBuildType(string alias);
		object BuildObject(string typeAlias, IDictionary<string, object> attributes);
	}
}
