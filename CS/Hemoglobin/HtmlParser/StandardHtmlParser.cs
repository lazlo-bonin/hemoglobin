namespace Hemoglobin
{
	public class StandardHtmlParser : HtmlParser
	{
		public StandardHtmlParser(AnonymousStructure anonymousStructure = AnonymousStructure.Dictionary)
			: base(new StandardHtmlBinder(), new ObjectBuilder(anonymousStructure))
		{ }

		public new ObjectBuilder Builder
		{
			get
			{
				return (ObjectBuilder)base.Builder;
			}
		}

		public new StandardHtmlBinder Binder
		{
			get
			{
				return (StandardHtmlBinder)base.Binder;
			}
		}
	}
}
