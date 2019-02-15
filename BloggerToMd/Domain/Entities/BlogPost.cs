using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Html2Markdown;

namespace BloggerToMd.Domain.Entities 
{
    internal class BlogPost
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

        public string ToMarkdown(string path)
        {
            var sb = new StringBuilder();
            sb.Append($@"---
path: ""{path}""
title: ""{Title}""
date: ""{Date:yy/MM/dd}""
originalUrl: ""{OriginalUrl}""
slug: ""{path}""
tags:
");
            foreach(var tag in Tags)
            {
                sb.Append($"    - {tag}\n");
            }

sb.Append(@"---
");
            sb.Append(Markdown);

            if (Comments.Any())
            {
                sb.Append(@"
## Comments

");
                foreach (var comment in Comments)
                {
                    sb.Append(comment.Markdown);
                    sb.Append($"by _{comment.Author}_ on {comment.Date:D}");
                }
            }
            return sb.ToString();
        }

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