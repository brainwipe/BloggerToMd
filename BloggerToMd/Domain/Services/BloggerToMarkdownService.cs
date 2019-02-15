using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using BloggerToMd.Domain.Entities;
using Html2Markdown;

namespace BloggerToMd
{
    public static class BloggerToMarkdownService
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
                var isComment = Comment.IsComment(entry, manager);
                if (isComment)
                {
                    comments.Add(Comment.From(entry, manager));
                }
                else
                {
                    posts.Add(BlogPost.From(entry, manager));
                }
            }

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

            var safeFilename = SafeFileName(post.OriginalUrl);
            var fileName = $"{post.Date:yyyy-MM-dd}-{safeFilename}.md";
            var path = $"/blog/{post.Date:yyyy}/{post.Date:MM}/{safeFilename}";
            var markdown = post.ToMarkdown(path);
            var fullPath = Path.Combine(folder, fileName);
            File.WriteAllText(fullPath, markdown);
        }

        private static string SafeFileName(string originalUrl) => 
            Path.GetFileName(originalUrl)
                .Replace(".html",string.Empty);
      
    }
}
