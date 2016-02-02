using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CsQuery;

namespace Hemoglobin
{
	public class HtmlParser : IHtmlParser
	{
		public IHtmlBinder Binder { get; private set; }
		public IObjectBuilder Builder { get; private set; }

		public HtmlParser(IHtmlBinder binder, IObjectBuilder builder)
		{
			Binder = binder;
			Builder = builder;
		}

		public object Parse(HtmlBinding binding, string html)
		{
			return Fetch(binding, CQ.Create(html));
		}

		public T Parse<T>(HtmlBinding binding, string html)
		{
			object untyped = Parse(binding, html);

			if (untyped is ICollection && typeof(ICollection).IsAssignableFrom(typeof(T)))
			{
				return (T)ObjectBuilder.ConvertCollection((ICollection)untyped, typeof(T));
			}
			else
			{
				return (T)untyped;
			}
		}

		private object Fetch(HtmlBinding binding, CQ parentNode)
		{
			CQ node = binding.Query == null ? parentNode : parentNode.Find(binding.Query);

			if (node.Length == 1)
			{
				object single = FetchSingle(binding, node);

				if (binding.Multiple)
				{
					return new object[] { single };
				}
				else
				{
					return single;
				}
			}
			else if (node.Length > 1)
			{
				if (!binding.Multiple)
				{
					throw new HemoglobinException(string.Format("Expecting single result but multiple nodes found for '{0}'.", binding.Field));
				}

				return node.Select(nodeDom => FetchSingle(binding, new CQ(nodeDom))).ToArray();
			}
			else // if (node.Length == 0)
			{
				if (binding.Multiple)
				{
					return new object[0];
				}
				else
				{
					return null;
				}
			}
		}

		private object FetchSingle(HtmlBinding binding, CQ node)
		{
			if (binding.Children != null)
			{
				return FetchStructure(binding, node);
			}
			else
			{
				return FetchField(binding, node);
			}
		}

		private object FetchField(HtmlBinding binding, CQ node)
		{
			if (binding.AccessorAlias == Binder.StructureAccessorAlias)
			{
				throw new HemoglobinException("Structure node without children.");
			}

			if (!Binder.HasAccessor(binding.AccessorAlias))
			{
				throw AccessorException.UnknownAlias(binding.AccessorAlias);
			}

			object field = Binder.Access(binding.AccessorAlias, binding.AccessorArguments, node);

			if (binding.ProcessorAlias != null)
			{
				if (!Binder.HasProcessor(binding.ProcessorAlias))
				{
					throw ProcessorException.UnknownAlias(binding.ProcessorAlias);
				}

				try
				{
					field = Binder.Process(binding.ProcessorAlias, binding.ProcessorArguments, field);
				}
				catch (Exception ex)
				{
					ProcessorException.ProcessingFailed(binding.ProcessorAlias, binding.Field, ex);
				}
			}

			return field;
		}

		private object FetchStructure(HtmlBinding binding, CQ node)
		{
			if (binding.AccessorAlias != Binder.StructureAccessorAlias)
			{
				throw new HemoglobinException("Non-structure node with children.");
			}

			string typeAlias;

			if (binding.AccessorArguments.Length == 0)
			{
				typeAlias = null;
			}
			else if (binding.AccessorArguments.Length == 1)
			{
				typeAlias = binding.AccessorArguments[0];
			}
			else
			{
				throw AccessorException.InvalidArgumentCount(Binder.StructureAccessorAlias);
			}

			if (!Builder.CanBuildType(typeAlias))
			{
				throw new HemoglobinException(string.Format("Cannot build object of type '{0}'.", typeAlias));
			}

			Dictionary<string, object> attributes = new Dictionary<string, object>(binding.Children.Count);

			foreach (HtmlBinding child in binding.Children)
			{
				attributes.Add(child.Field, Fetch(child, node));
			}

			object structure = Builder.BuildObject(typeAlias, attributes);

			if (binding.ProcessorAlias != null)
			{
				if (!Binder.HasProcessor(binding.ProcessorAlias))
				{
					throw ProcessorException.UnknownAlias(binding.ProcessorAlias);
				}

				try
				{
					structure = Binder.Process(binding.ProcessorAlias, binding.ProcessorArguments, structure);
				}
				catch (Exception ex)
				{
					ProcessorException.ProcessingFailed(binding.ProcessorAlias, binding.Field, ex);
				}
			}

			return structure;
		}
	}
}
