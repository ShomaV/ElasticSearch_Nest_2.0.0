using System;

namespace ElasticSearch_NEST
{
    using Nest;

    [ElasticsearchType(Name = "blog_post", IdProperty = "Id")]
    public class BlogPost
    {
        [String(Name = "id", Index = FieldIndexOption.NotAnalyzed)]
        public Guid? Id { get; set; }

        [String(Name = "title", Index = FieldIndexOption.Analyzed)]
        public string Title { get; set; }

        [String(Name = "body", Index = FieldIndexOption.Analyzed)]
        public string Body { get; set; }

        [Nested(Name = "author",IncludeInParent = true)]
        public Author Author { get; set; }

        public override string ToString()
        {
            return string.Format("Id: '{0}', Title: '{1}', Body: '{2}'", Id, Title, Body);
        }
    }
}
