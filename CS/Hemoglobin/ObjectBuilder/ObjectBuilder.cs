using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Hemoglobin
{
	public enum AnonymousStructure
	{
		Dictionary,
		FixedObject,
		ExpandoObject
	}

	public class ObjectBuilder : IObjectBuilder
	{
		public AnonymousStructure AnonymousStructure { get; private set; }
		public Dictionary<string, Type> Types { get; private set; }

		public ObjectBuilder(AnonymousStructure anonymousStructure)
		{
			AnonymousStructure = anonymousStructure;
			Types = new Dictionary<string, Type>();
		}

		public ObjectBuilder(AnonymousStructure anonymousStructure, IDictionary<string, Type> types)
		{
			AnonymousStructure = anonymousStructure;
			Types = new Dictionary<string, Type>(types);
		}

		public bool CanBuildType(string alias)
		{
			// Can build any type by fallbacking to the anonymous structure.
			return true;
		}

		public object BuildObject(string typeAlias, IDictionary<string, object> attributes)
		{
			if (Types.ContainsKey(typeAlias))
			{
				return BuildObject(Types[typeAlias], attributes);
			}
			else
			{
				return BuildAnonymousStructure(attributes);
			}
		}

		private object BuildObject(Type type, IDictionary<string, object> attributes)
		{
			object typed = Activator.CreateInstance(type);

			foreach (KeyValuePair<string, object> attribute in attributes)
			{
				var name = attribute.Key;

				var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

				var field = type.GetField(name, bindingFlags);
				var property = type.GetProperty(name, bindingFlags);

				if (field == null && property == null)
				{
					continue;
				}

				object value;

				if (attribute.Value is ICollection)
				{
					Type destinationType;

					if (field != null)
					{
						destinationType = field.FieldType;
					}
					else // if (property != null)
					{
						destinationType = property.PropertyType;
					}

					value = ConvertCollection((ICollection)attribute.Value, destinationType);
				}
				else
				{
					value = attribute.Value;
				}

				if (field != null)
				{
					field.SetValue(typed, value);
				}
				else // if (property != null)
				{
					property.SetValue(typed, value);
				}
			}

			return typed;
		}

		// TODO: Move to own class
		// TODO: Generic version 
		public static object ConvertCollection(ICollection sourceCollection, Type destinationCollectionType)
		{
			var sourceCollectionType = sourceCollection.GetType();

			if (destinationCollectionType.IsAssignableFrom(sourceCollectionType))
			{
				return sourceCollection;
			}

			// TODO?: Infer sourceElementType here by checking if it implements ICollection<T> and finding T
			// Then simply check if destinationElementType.IsAssignableFrom(sourceElementType)
			var sourceArray = sourceCollection.OfType<object>().ToArray();

			if (destinationCollectionType.IsArray)
			{
				var destinationElementType = destinationCollectionType.GetElementType();

				var destinationArray = Array.CreateInstance(destinationElementType, sourceArray.Length);
				
				sourceArray.CopyTo(destinationArray, 0);
				
				return destinationArray;
			}
			else
			{
				var destinationGenericArguments = destinationCollectionType.GetGenericArguments();

				// TODO: Improve to work for any IEnumerable constructor regardless of type generic

				if (destinationGenericArguments.Length == 1)
				{
					var destinationElementType = destinationGenericArguments[0];
					
					var destinationEnumerableType = typeof(IEnumerable<>).MakeGenericType(destinationElementType);

					var destinationEnumerableConstructor = destinationCollectionType.GetConstructor(new Type[] { destinationEnumerableType });

					if (destinationEnumerableConstructor != null)
					{
						var destinationArray = Array.CreateInstance(destinationElementType, sourceArray.Length);

						sourceArray.CopyTo(destinationArray, 0);
					
						var destinationCollection = destinationEnumerableConstructor.Invoke(new object[] { destinationArray });

						return destinationCollection;
					}
				}
            }

			throw new HemoglobinException(string.Format("Could not convert collection type from '{0}' to '{1}'.", sourceCollectionType, destinationCollectionType));
		}

		private object BuildAnonymousStructure(IDictionary<string, object> properties)
		{
			switch (AnonymousStructure)
			{
				case AnonymousStructure.Dictionary: return BuildDictionary(properties);
				case AnonymousStructure.FixedObject: return BuildFixedObject(properties);
				case AnonymousStructure.ExpandoObject: return BuildExpandoObject(properties);
				default: throw new HemoglobinException(string.Format("Unknown anonymous structure type: '{0}'", AnonymousStructure));
			}
		}

		private static IDictionary<string, object> BuildDictionary(IDictionary<string, object> properties)
		{
			return new Dictionary<string, object>(properties);
		}

		private static FixedObject BuildFixedObject(IDictionary<string, object> properties)
		{
			return new FixedObject(properties);
		}

		private static ExpandoObject BuildExpandoObject(IDictionary<string, object> properties)
		{
			var expando = new ExpandoObject();

			var expandoAsDictionary = (IDictionary<string, object>)expando;

			foreach (var property in properties)
			{
				expandoAsDictionary.Add(property);
			}

			return expando;
		}
	}
}
