Articles[]: struct(Article) from article.post.block
{
	Hed: text from .hed
	Topic: text from .topic
	Date: text from .date
	Contributors[]: text from .contributor
}
