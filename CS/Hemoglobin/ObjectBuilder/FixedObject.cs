using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Hemoglobin
{
	public class FixedObject : DynamicObject
	{
		private readonly Dictionary<string, object> dictionary;

		public FixedObject(IDictionary<string, object> dictionary)
		{
			this.dictionary = new Dictionary<string, object>(dictionary);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return dictionary.TryGetValue(binder.Name, out result);
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (dictionary.ContainsKey(binder.Name))
			{
				dictionary[binder.Name] = value;

				return true;
			}
			else
			{
				return false;
			}
		}

		// TODO: Provide as extension method for Dictionary<TK, TV> and ExpandoObject for uniformity
		// With indentLevel param (default 0) and StringBuilder implementation ffs
		// Oh and ideally recursive support
		public string ToDebugString()
		{
			return GetType() + "\n{\n\t" + string.Join("\n\t", dictionary.Select(kv => kv.Key + " = " + kv.Value).ToArray()) + "\n}";
		}
	}
}
