Link[]: struct(Link) from .link
{
	Rank: text from .rank
	Title: text from a.title
	Link: attr(href) from a.title
	Domain: text from .domain
	CommentCount: text as commentCount from a.comments
}