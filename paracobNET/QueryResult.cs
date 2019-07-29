using System;
using System.Collections.Generic;

namespace paracobNET
{
    public class QueryResult : IQueriable
    {
        public List<IParam> Results { get; }

        public QueryResult(List<IParam> results)
        {
            Results = results;
        }

        public QueryResult Query(string path)
        {
            List<IParam> results = new List<IParam>();
            foreach (var param in Results)
            {
                if (param is ParamStruct s)
                    results.AddRange(s.Query(path).Results);
                else if (param is ParamList l)
                    results.AddRange(l.Query(path).Results);
            }
            return new QueryResult(results);
        }

        public QueryResult Query(string path, IDictionary<string, ulong> labels)
        {
            List<IParam> results = new List<IParam>();
            foreach (var param in Results)
            {
                if (param is ParamStruct s)
                    results.AddRange(s.Query(path, labels).Results);
                else if (param is ParamList l)
                    results.AddRange(l.Query(path, labels).Results);
            }
            return new QueryResult(results);
        }
    }
}
