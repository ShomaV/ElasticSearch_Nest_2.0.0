using System;

namespace ElasticSearch_NEST
{
    using System.Linq;
    using Nest;

    class Program
    {
        static void Main(string[] args)
        {
            var local = new Uri("http://localhost:9200");
            string indexName = "blog_post_index";
            var settings = new ConnectionSettings(local).DefaultIndex(indexName);
            var elastic = new ElasticClient(settings);

            var res = elastic.ClusterHealth();

            Console.WriteLine(res.Status);

            var blogPost = new BlogPost
            {
                Id = Guid.NewGuid(),
                Title = "First blog post",
                Body = "This is very long blog post!"
            };

            if (!elastic.IndexExists(indexName).Exists)
            {
                var createIndexResponse = elastic.CreateIndex(indexName);
                Console.WriteLine("createIndexResponse=" + createIndexResponse.IsValid);
            }

            IIndexResponse indexResponse = elastic.Index(blogPost, i => i
                .Index(indexName)
                .Type(typeof(BlogPost))
                .Id(1)
                .Refresh());
            Console.WriteLine("IIndexResponse=" + indexResponse.IsValid);


            //insert 10 documents
            for (var i = 2; i < 12; i++)
            {
                var blogPostNew = new BlogPost
                {
                    Id = Guid.NewGuid(),
                    Title = string.Format("title {0:000}", i),
                    Body = string.Format("This is {0:000} very long blog post!", i)
                };
                IIndexResponse bulkIndexReponse = elastic.Index(blogPostNew, p => p
                    .Type(typeof(BlogPost))
                    .Id(i)
                    .Refresh());
                Console.WriteLine("bulk IIndexResponse=" + bulkIndexReponse.IsValid);
            }

            //Get document by id
            var result = elastic.Get<BlogPost>(new GetRequest(indexName, typeof(BlogPost), 16));

            Console.WriteLine("Document id:" + result.Id);
            Console.WriteLine("Document fields:" + result.Fields);
            Console.WriteLine("Document Type:" + result.Type);
            Console.WriteLine("Document Found Status:" + result.Found);

            //delete document by id
            //var deleteResult = elastic.Delete(new DeleteRequest(indexName, typeof(BlogPost), 1));
            //Console.WriteLine(deleteResult.Found);

            //Query search queries for match all
            var searchResult = elastic.Search<BlogPost>(sr => sr
                .From(0)
                .Size(5)
                .Query(q => q.MatchAll())
                .Sort(ss => ss
                        .Ascending(p => p.Title)
                        .Field(f => f.Field(ff => ff.Title)))
                    );

            Console.WriteLine("Search results for Match All!! ==>");
            Console.WriteLine(searchResult.Hits.Count());
            foreach (var hit in searchResult.Hits)
            {
                Console.WriteLine(hit.Source);
            }

            //Query search results using Match
            var blogPostsForSearch = new[]
                {
                    new BlogPost {
                        Id = Guid.NewGuid(),
                        Title = "test post 123",
                        Body = "1" },
                    new BlogPost
                    {
                        Id = Guid.NewGuid(),
                        Title = "test something 123",
                        Body = "2"
                    },
                    new BlogPost
                    {
                        Id = Guid.NewGuid(),
                        Title = "read this post",
                        Body = "3"
                    }
                };
            var id = 15;
            foreach (var blogPostSearch in blogPostsForSearch)
            {

                var insertRes = elastic.Index(blogPostSearch, p => p
                    .Id(++id)
                    .Refresh());
                Console.WriteLine("Match SearchResults IIndexResponse=" + insertRes.IsValid);
            }

            var searchMatch = elastic.Search<BlogPost>(es => es
                                                        .Query(q => q
                                                                .Match(m => m
                                                                    .Field(f => f.Title)
                                                                        .Query("test post 123"))));

            Console.WriteLine("Search results for Match!! ==>");
            Console.WriteLine(searchMatch.Hits.Count());
            foreach (var hit in searchMatch.Hits)
            {
                Console.WriteLine(hit.Source);
            }

            //Match with AND Operator
            var searchMatchAnd = elastic.Search<BlogPost>(es => es
                                                        .Query(q => q
                                                                .Match(m => m
                                                                    .Field(f => f.Title)
                                                                        .Query("test post 123")
                                                                        .Operator(Operator.And))));

            Console.WriteLine("Search results for Match!! ==>");
            Console.WriteLine(searchMatchAnd.Hits.Count());
            foreach (var hit in searchMatchAnd.Hits)
            {
                Console.WriteLine(hit.Source);
            }

            //MinimumShouldMatch
            var searchMinMatch = elastic.Search<BlogPost>(es => es
                                                       .Query(q => q
                                                               .Match(m => m
                                                                   .Field(f => f.Title)
                                                                       .Query("test post 123")
                                                                       .Operator(Operator.Or)
                                                                       .MinimumShouldMatch(2))));

            Console.WriteLine("Search results for Min Match!! ==>");
            Console.WriteLine(searchMinMatch.Hits.Count());
            foreach (var hit in searchMinMatch.Hits)
            {
                Console.WriteLine(hit.Source);
            }

            //Bool Query
            var boolQuerySearchResult = elastic.Search<BlogPost>(es => es
                                            .Query(qu => qu
                                            .Bool(b => b
                                                .Must(m =>
                                                        m.Match(mt => mt.Field(f => f.Title).Query("title")) &&
                                                        m.Match(mt2 => mt2.Field(f => f.Body).Query("002")))))
                                                        .Sort(so => so.Field(fe => fe.Field(fe1 => fe1.Title))
                                                        .Ascending(p => p.Title)));

            Console.WriteLine("Search results for Bool with Must!! ==>");
            Console.WriteLine(boolQuerySearchResult.Hits.Count());
            foreach (var hit in boolQuerySearchResult.Hits)
            {
                Console.WriteLine(hit.Source);
            }

            //Using replacing Must with should (or)
            var boolQuerySearchResultShould = elastic.Search<BlogPost>(es => es
                                            .Query(qu => qu
                                            .Bool(b => b
                                                .Should(m =>
                                                        m.Match(mt => mt.Field(f => f.Title).Query("title")) ||
                                                        m.Match(mt2 => mt2.Field(f => f.Body).Query("002")))))
                                                        .Sort(so => so.Field(fe => fe.Field(fe1 => fe1.Title))
                                                        .Ascending(p => p.Title)));

            Console.WriteLine("Search results for Bool with Should!! ==>");
            Console.WriteLine(boolQuerySearchResultShould.Hits.Count());
            foreach (var hit in boolQuerySearchResultShould.Hits)
            {
                Console.WriteLine(hit.Source);
            }

            //Using bool with MUST NOT
            var boolQuerySearchResultMustNot = elastic.Search<BlogPost>(es => es
                                           .Query(qu => qu
                                           .Bool(b => b
                                               .Should(m =>
                                                       m.Match(mt => mt.Field(f => f.Title).Query("title")) ||
                                                       m.Match(mt2 => mt2.Field(f => f.Body).Query("002")))
                                                .Must(ms => ms
                                                        .Match(mt3 => mt3.Field(fi => fi.Body).Query("this")))
                                                .MustNot(mn => mn
                                                        .Match(mt4 => mt4.Field(fi => fi.Body).Query("003")))))
                                                       .Sort(so => so.Field(fe => fe.Field(fe1 => fe1.Title))
                                                       .Ascending(p => p.Title)));

            Console.WriteLine("Search results for Bool with MUST NOT!! ==>");
            Console.WriteLine(boolQuerySearchResultMustNot.Hits.Count());
            foreach (var hit in boolQuerySearchResultMustNot.Hits)
            {
                Console.WriteLine(hit.Source);
            }

            //Using the above query with bitwise operator
            var boolQuerySearchResultBitwise = elastic.Search<BlogPost>(es => es
                .Query(q =>
                    (q.Match(mt1 => mt1.Field(f1 => f1.Title).Query("title")) ||
                    q.Match(mt2 => mt2.Field(f2 => f2.Body).Query("002")))
                    && (q.Match(mt3 => mt3.Field(fe3 => fe3.Body).Query("this")))
                    && (!q.Match(mt4 => mt4.Field(fe4 => fe4.Body).Query("003")))));

            Console.WriteLine("Search results for Bool with Bitwise operator!! ==>");
            Console.WriteLine(boolQuerySearchResultBitwise.Hits.Count());
            foreach (var hit in boolQuerySearchResultBitwise.Hits)
            {
                Console.WriteLine(hit.Source);
            }
            //Nested Types and Nested Query
            Console.WriteLine("*******Nested Types and Nested Query*************");

            var author1 = new Author { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
            var author2 = new Author { Id = Guid.NewGuid(), FirstName = "Notjohn", LastName = "Doe" };
            var author3 = new Author { Id = Guid.NewGuid(), FirstName = "John", LastName = "Notdoe" };
            
            var blogPostWithAuthor = new[]
            {
                new BlogPost { Id = Guid.NewGuid(), Title = "test post 1", Body = "1" , Author = author1 },
                new BlogPost { Id = Guid.NewGuid(), Title = "test post 2", Body = "2" , Author = author2 },
                new BlogPost { Id = Guid.NewGuid(), Title = "test post 3", Body = "3" , Author = author3 }
            };

            foreach (var blogPostAuthor in blogPostWithAuthor)
            {
                var resindex = elastic.Index(blogPostAuthor, p => p
                    .Id(blogPostAuthor.Id.ToString())
                    .Refresh());
                Console.WriteLine("Match SearchResults IIndexResponse=" + resindex.IsValid);
            }

            Console.WriteLine("*******Nested Query*************");
            var nestedQuery = elastic.Search<BlogPost>(es => es
                .Query(q => q
                    .Nested(n => n
                        .Path(b => b.Author)
                        .Query(nq =>
                            nq.Match(m1 => m1.Field(f1 => f1.Author.FirstName).Query("John")) &&
                            nq.Match(m2 => m2.Field(f2 => f2.Author.LastName).Query("Doe"))))
                            ));

            Console.WriteLine(nestedQuery.IsValid);
            Console.WriteLine(nestedQuery.Hits.Count());
            foreach (var hit in nestedQuery.Hits)
            {
                Console.WriteLine(hit.Source);
            }
            Console.ReadLine();
        }
    }
}
