using System;
using System.Xml;
using Html2Markdown;

namespace BloggerToMd.Domain.Entities
{
    internal class Comment
    {
        private Comment(string author, DateTimeOffset date, string markdown, string originalPost)
        {
            Author = author;
            Date = date;
            Markdown = markdown;
            OriginalPost = originalPost;
        }

        public string Author { get; }
        public DateTimeOffset Date { get; }
        public string Markdown { get; }
        public string OriginalPost { get; }

        public static Comment From(XmlNode comment, XmlNamespaceManager manager)
        {
            var converter = new Converter();
            var author = comment.SelectSingleNode("atom:author", manager).InnerText;
            var date = DateTimeOffset.Parse(comment.SelectSingleNode("atom:published", manager).InnerText);
            var content = comment.SelectSingleNode("atom:content", manager).InnerText;
            var markdown = converter.Convert(content);
            var originalPost = comment.SelectSingleNode("thr:in-reply-to/@href", manager).InnerText;
            return new Comment(author, date, markdown, originalPost);
        }

        public static bool IsComment(XmlNode entry, XmlNamespaceManager manager)
        {
            var comment =
                entry.SelectSingleNode(
                    "atom:category[@scheme='http://schemas.google.com/g/2005#kind']/@term", manager).InnerText;

            return comment == "http://schemas.google.com/blogger/2008/kind#comment";
        }
    }
}