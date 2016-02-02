using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hemoglobin
{
	// TODO: Review and subclass all exceptions in this class.

	public sealed class HtmlBinding
	{
		public string Field { get; set; }
		public bool Multiple { get; set; }
		public string AccessorAlias { get; set; }
		public string[] AccessorArguments { get; set; }
		public string ProcessorAlias { get; set; }
		public string[] ProcessorArguments { get; set; }
		public string Query { get; set; }
		public List<HtmlBinding> Children { get; private set; }

		public static HtmlBinding Load(string path)
		{
			return Parse(File.ReadAllText(path));
		}

		public static HtmlBinding[] LoadAll(string path)
		{
			return ParseAll(File.ReadAllText(path));
		}

		public void Save(string path)
		{
			File.WriteAllText(ToString(), path);
		}

		public static HtmlBinding Parse(string text)
		{
			HtmlBinding[] schemas = ParseAll(text);

			if (schemas.Length == 0)
			{
				throw new HemoglobinException("No root binding found.");
			}

			if (schemas.Length > 1)
			{
				throw new HemoglobinException("Multiple root binding found.");
			}

			return schemas[0];
		}

		public static HtmlBinding[] ParseAll(string text)
		{
			StringReader reader = new StringReader(text);

			var parents = new Stack<HtmlBinding>();
			HtmlBinding previous = null;

			string line;

			var schemas = new List<HtmlBinding>();
			
			while ((line = reader.ReadLine()) != null)
			{
				int commentStart = line.IndexOf("//");

				if (commentStart >= 0)
				{
					line = line.Substring(0, commentStart);
				}

				line = line.Trim();

				if (line == string.Empty)
				{
					continue;
				}

				if (line == "{")
				{
					if (previous == null)
					{
						throw new HemoglobinException("Opening bracket without parent.");
					}

					parents.Push(previous);
					previous = null;
				}
				else if (line == "}")
				{
					if (parents.Count == 0)
					{
						throw new HemoglobinException("Closing bracket without parent.");
					}

					parents.Pop();
					previous = null;
				}
				else
				{
					HtmlBinding current = ParseLine(line);
					
					if (parents.Count == 0)
					{
						schemas.Add(current);
					}
					else
					{
						HtmlBinding parent = parents.Peek();

						if (parent.Children == null)
						{
							parent.Children = new List<HtmlBinding>();
						}

						parent.Children.Add(current);
					}

					previous = current;
				}
			}

			reader.Dispose();

			return schemas.ToArray();
		}

		private static HtmlBinding ParseLine(string line)
		{
			string regex =
				@"(?<Field>\w+)(?<Multiple>\s*\[\s*\])?" + // Field identifier with optional brackets
				@"\s*:\s*" + // Colon separator
				@"(?:(?<AccessorAlias>\w+)" + ArgumentRegex("AccessorArguments") + @")" + // Accessor alias with optional argument list
				@"(?:\s+as\s+(?<ProcessorAlias>\w+)" + ArgumentRegex("ProcessorArguments") + @")?" + // Optional processor alias with optional argument list
				@"(?:\s+from\s+(?<Query>.+))?"; // Optional query string

			Match match = Regex.Match(line, regex);

			if (!match.Success)
			{
				throw new HemoglobinException(string.Format("Invalid binding syntax: '{0}'", line));
			}

			var groups = match.Groups;

			HtmlBinding binding = new HtmlBinding()
			{
				Field = groups["Field"].Value.Trim(),
				Multiple = groups["Multiple"].Success,
				AccessorAlias = groups["AccessorAlias"].Value.Trim(),
				AccessorArguments = groups["AccessorArguments"].Success ? groups["AccessorArguments"].Value.Split(',').Select(s => s.Trim()).ToArray() : new string[0],
				ProcessorAlias = groups["ProcessorAlias"].Success ? groups["ProcessorAlias"].Value.Trim() : null,
				ProcessorArguments = groups["ProcessorArguments"].Success ? groups["ProcessorArguments"].Value.Split(',').Select(s => s.Trim()).ToArray() : (groups["ProcessorAlias"].Success ? new string[0] : null),
                Query = groups["Query"].Success ? groups["Query"].Value.Trim() : null,
			};

			return binding;
		}

		private static string ArgumentRegex(string captureGroupName)
		{
			return @"(?:\s*\(\s*(?<" + captureGroupName + @">\w+(?:\s*,\s*\w+)*)\s*\))?";
        }

		public override string ToString()
		{
			var text = new StringBuilder();

            WriteTo(text);

			return text.ToString();
		}

		private void WriteTo(StringBuilder text, int indentLevel = 0)
		{
			text.Append('\t', indentLevel);
			text.AppendLine(LineToString());

			if (Children != null)
			{
				text.Append('\t', indentLevel);
				text.AppendLine("{");
				
				foreach (HtmlBinding child in Children)
				{
					child.WriteTo(text, indentLevel + 1);
				}

				text.Append('\t', indentLevel);
				text.AppendLine("}");
			}
		}

		private string LineToString()
		{
			if (Field == null) throw new HemoglobinException("Missing field.");
			if (!IsWord(Field)) throw new HemoglobinException("Invalid field.");

			if (AccessorAlias == null) throw new HemoglobinException("Missing accessor alias.");
			if (!IsWord(AccessorAlias)) throw new HemoglobinException("Invalid accessor alias.");
			if (AccessorArguments != null && AccessorArguments.Any(s => !IsWord(s))) throw new HemoglobinException("Invalid accessor argument.");

			if (ProcessorAlias != null)
			{
				if (!IsWord(ProcessorAlias)) throw new HemoglobinException("Invalid processor alias.");
				if (ProcessorArguments != null && ProcessorArguments.Any(s => !IsWord(s))) throw new HemoglobinException("Invalid processor argument.");
			}
			else if (ProcessorArguments != null) throw new HemoglobinException("Processor arguments without alias.");

			var line = new StringBuilder();

			line.Append(Field);

			if (Multiple)
			{
				line.Append("[]");
			}

			line.Append(": ");
			
			line.Append(AccessorAlias);

			if (AccessorArguments != null && AccessorArguments.Length > 0)
			{
				line.Append("(");
				line.Append(string.Join(", ", AccessorArguments));
				line.Append(")");
			}

			if (ProcessorAlias != null)
			{
				line.Append(" as ");
				line.Append(ProcessorAlias);

				if (ProcessorArguments != null && ProcessorArguments.Length > 0)
				{
					line.Append("(");
					line.Append(string.Join(", ", ProcessorArguments));
					line.Append(")");
				}
			}

			if (Query != null)
			{
				line.Append(" from ");
				line.Append(Query);
			}

			return line.ToString();
		}

		private static bool IsWord(string text)
		{
			return Regex.IsMatch(text, @"\w+");
		}
	}
}
