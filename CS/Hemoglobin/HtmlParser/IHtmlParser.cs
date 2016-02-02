namespace Hemoglobin
{
	interface IHtmlParser
	{
		IHtmlBinder Binder { get; }
		IObjectBuilder Builder { get; }
		object Parse(HtmlBinding binding, string html);
	}
}
