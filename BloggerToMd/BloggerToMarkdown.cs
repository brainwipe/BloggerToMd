using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Html2Markdown;

namespace BloggerToMd
{
    public static class BloggerToMarkdown
    {
        public static void Convert(string bloggerFileName)
        {
            var document = Load(bloggerFileName);
            var manager = BloggerNameSpaceManager(document);
            var entries = document.DocumentElement.SelectNodes("atom:entry[contains(atom:id, 'post')]", manager);

            var folder = Path.GetDirectoryName(bloggerFileName);
            var posts = new List<BlogPost>();
            var comments = new List<Comment>();

            foreach (XmlNode entry in entries)
            {
                var isComment = IsComment(entry, manager);
                if (isComment)
                {
                    comments.Add(Comment.From(entry, manager));
                }
                else
                {
                    posts.Add(BlogPost.From(entry, manager));
                }
            }

            // Save out
            foreach (var post in posts)
            {
                var postComments = comments.Where(c => post.OriginalUrl == c.OriginalPost).OrderBy(c => c.Date);
                post.Comments.AddRange(postComments);

                var isDraft = post.IsDraft ? "Draft" : $"{post.OriginalUrl}";

                Debug.WriteLine($"{post.Title} | {isDraft} | Comments: {post.Comments.Count}");
                 SaveToFile(post, folder);
            }
        }

        private static XmlDocument Load(string bloggerFileName)
        {
            var doc = new XmlDocument();
            doc.Load(bloggerFileName);
            return doc;
        }

        private static XmlNamespaceManager BloggerNameSpaceManager(XmlDocument bloggerDocument)
        {
            var manager = new XmlNamespaceManager(bloggerDocument.NameTable);
            manager.AddNamespace("openSearch", "http://a9.com/-/spec/opensearchrss/1.0/");
            manager.AddNamespace("gd", "http://schemas.google.com/g/2005");
            manager.AddNamespace("thr", "http://purl.org/syndication/thread/1.0");
            manager.AddNamespace("georss", "http://www.georss.org/georss");
            manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            manager.AddNamespace("app", "http://purl.org/atom/app#");
            return manager;
        }

        private static void SaveToFile(BlogPost post, string folder)
        {
            if (post.IsDraft)
            {
                return;
            }

            var fileName = $"{post.Date:yyyy-MM-dd}-{SafeFileName(post.OriginalUrl)}.md";
            var sb = new StringBuilder();
            sb.Append($@"Title: ""{post.Title}""
Date: {post.Date:dd/MM/yy}
OriginalUrl: ""{post.OriginalUrl}""
---
");
            sb.Append(post.Markdown);
            sb.Append(@"
## Comments

");
            foreach (var comment in post.Comments)
            {
                sb.Append(comment.Markdown);
                sb.Append($"by _{comment.Author}_ on {comment.Date:D}");
            }

            var fullPath = Path.Combine(folder, fileName);
            File.WriteAllText(fullPath, sb.ToString());
        }

        private static string SafeFileName(string originalUrl) => 
            Path.GetFileName(originalUrl)
                .Replace(".html",string.Empty);

        private static bool IsComment(XmlNode entry, XmlNamespaceManager manager)
        {
            var comment =
                entry.SelectSingleNode(
                    "atom:category[@scheme='http://schemas.google.com/g/2005#kind']/@term", manager).InnerText;

            return comment == "http://schemas.google.com/blogger/2008/kind#comment";
        }

        private class Comment
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
        }

        private class BlogPost
        {
            private BlogPost(string title, DateTimeOffset date, string[] tags, string markdown, string originalUrl, bool isDraft)
            {
                Title = title;
                Date = date;
                Tags = tags;
                Markdown = markdown;
                OriginalUrl = originalUrl;
                IsDraft = isDraft;
                Comments = new List<Comment>();
            }

            public string Title { get; }
            public DateTimeOffset Date { get; }
            public string[] Tags { get; }
            public string Markdown { get; }
            public string OriginalUrl { get;}
            public bool IsDraft { get; }
            public List<Comment> Comments { get; }

            public static BlogPost From(XmlNode blogPost, XmlNamespaceManager manager)
            {
                var converter = new Converter();
                var isDraft = blogPost.SelectSingleNode("app:control", manager) != null;
                var originalUrl = isDraft ? "" : blogPost.SelectSingleNode("atom:link[@rel='alternate']/@href", manager).InnerText;

                var title = blogPost.SelectSingleNode("atom:title", manager).InnerText;
                var date = DateTimeOffset.Parse(blogPost.SelectSingleNode("atom:published", manager).InnerText);
                var tagNodes = blogPost.SelectNodes("atom:category[@scheme='http://www.blogger.com/atom/ns#']/@term",
                    manager);


                var tags = new List<string>();
                foreach (XmlNode tag in tagNodes)
                {
                    tags.Add(tag.Value);
                }

                var content = blogPost.SelectSingleNode("atom:content", manager).InnerText;
                var markdown = converter.Convert(content);

                return new BlogPost(title, date, tags.ToArray(), markdown, originalUrl, isDraft);
            }
        }
    }
}
