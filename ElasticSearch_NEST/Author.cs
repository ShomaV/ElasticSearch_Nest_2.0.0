using System;

namespace ElasticSearch_NEST
{
    using Nest;
    [ElasticsearchType(Name = "author", IdProperty = "Id")]
    public class Author
    {
        [String(Name = "id", Index = FieldIndexOption.NotAnalyzed)]
        public Guid? Id { get; set; }

        [String(Name = "first_name", Index = FieldIndexOption.Analyzed)]
        public string FirstName { get; set; }

        [String(Name = "last_name", Index = FieldIndexOption.Analyzed)]
        public string LastName { get; set; }

        public override string ToString()
        {
            return string.Format("Id: '{0}', First name: '{1}', Last Name: '{2}'", Id, FirstName, LastName);
        }
    }
}
