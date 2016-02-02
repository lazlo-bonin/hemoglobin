using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CsQuery;

namespace Hemoglobin
{
	public class StandardHtmlBinder : HtmlBinder
	{
		public StandardHtmlBinder() : base()
		{
			StructureAccessorAlias = "struct";

			Accessors.Add("html", AccessHtml);
			Accessors.Add("text", AccessText);
			Accessors.Add("attr", AccessAttr);
			Accessors.Add("data", AccessData);
			Accessors.Add("css", AccesCss);
			Accessors.Add("class", AccessClass);

			Processors.Add("byte", ProcessByConverting<byte>);
			Processors.Add("sbyte", ProcessByConverting<sbyte>);
			Processors.Add("short", ProcessByConverting<short>);
			Processors.Add("ushort", ProcessByConverting<ushort>);
			Processors.Add("int", ProcessByConverting<int>);
			Processors.Add("uint", ProcessByConverting<uint>);
			Processors.Add("long", ProcessByConverting<long>);
			Processors.Add("ulong", ProcessByConverting<ulong>);
			Processors.Add("float", ProcessByConverting<float>);
			Processors.Add("double", ProcessByConverting<double>);
			Processors.Add("decimal", ProcessByConverting<decimal>);
			Processors.Add("datetime", ProcessByConverting<DateTime>);
			Processors.Add("timespan", ProcessByConverting<TimeSpan>);
			Processors.Add("bool", ProcessAsBool);
		}

		#region Accessors

		private object AccessHtml(CQ node, string[] arguments, string alias)
		{
			if (arguments.Length == 0)
			{
				return node.Html();
			}
			else
			{
				throw AccessorException.InvalidArgumentCount(alias);
			}
		}

		private object AccessText(CQ node, string[] arguments, string alias)
		{
			if (arguments.Length == 0)
			{
				return node.Text().Trim();
			}
			else
			{
				throw AccessorException.InvalidArgumentCount(alias);
			}
		}

		private object AccessAttr(CQ node, string[] arguments, string alias)
		{
			if (arguments.Length == 0)
			{
				return node[0].Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			}
			else if (arguments.Length == 1)
			{
				return node.Attr(arguments[0]);
			}
			else
			{
				throw AccessorException.InvalidArgumentCount(alias);
			}
		}

		private object AccessData(CQ node, string[] arguments, string alias)
		{
			if (arguments.Length == 0)
			{
				return node[0].Attributes.Where(kvp => kvp.Key.StartsWith("data-")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			}
			else if (arguments.Length == 1)
			{
				return node.Data(arguments[0]);
			}
			else
			{
				throw AccessorException.InvalidArgumentCount(alias);
			}
		}

		private object AccesCss(CQ node, string[] arguments, string alias)
		{
			if (arguments.Length == 1)
			{
				return node.Css(arguments[0]);
			}
			else
			{
				throw AccessorException.InvalidArgumentCount(alias);
			}
		}

		private object AccessClass(CQ node, string[] arguments, string alias)
		{
			if (arguments.Length == 0)
			{
				return node.Attr("class").Split(' ').Select(c => c.Trim());
			}
			else
			{
				throw AccessorException.InvalidArgumentCount(alias);
			}
		}

		#endregion

		#region Processors

		protected object ProcessBase<T>(Processor processSingle, object value, string[] arguments, string alias)
		{
			if (value is T || value is ICollection<T>)
			{
				return value;
			}
			else if (value is ICollection)
			{
				var values = new List<object>();

				foreach (object singleValue in (ICollection)value)
				{
					values.Add(processSingle(value, arguments, alias));
				}

				return values.ToArray();
			}
			else
			{
				return processSingle(value, arguments, alias);
			}
		}

		protected object ProcessByConverting<T>(object value, string[] arguments, string alias)
		{
			return ProcessBase<T>(ProcessByConvertingSingle<T>, value, arguments, alias);
		}

		private object ProcessByConvertingSingle<T>(object value, string[] arguments, string alias)
		{
			if (arguments.Length != 0)
			{
				throw ProcessorException.InvalidArgumentCount(alias);
			}

			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

			if (!converter.CanConvertFrom(value.GetType()))
			{
				throw ProcessorException.InvalidInput(alias);
			}

			return converter.ConvertFrom(value);
		}

		private object ProcessAsBool(object value, string[] arguments, string alias)
		{
			return ProcessBase<bool>(ProcessAsBoolSingle, value, arguments, alias);
		}

		private object ProcessAsBoolSingle(object value, string[] arguments, string alias)
		{
			if (arguments.Length != 0)
			{
				throw ProcessorException.InvalidArgumentCount(alias);
			}

			TypeConverter converter = TypeDescriptor.GetConverter(typeof(bool));

			if (converter.CanConvertFrom(value.GetType()))
			{
				return converter.ConvertFrom(value);
			}
			else if (value is IConvertible)
			{
				try
				{
					return Convert.ToDouble(value) > 0;
				}
				catch (FormatException)
				{
					throw ProcessorException.InvalidInput(alias);
				}
			}
			else
			{
				throw ProcessorException.InvalidInput(alias);
			}
		}

		#endregion
	}
}
