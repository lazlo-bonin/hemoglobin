SearchResult[]: struct(SearchResult) from .search-result
{
	Title: text from a.search-title
	Link: attr(href) from a.search-title
	CommentCount: text as commentCount from a.search-comments
}