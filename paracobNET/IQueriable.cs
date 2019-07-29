using System;
using System.Collections.Generic;
using System.Text;

namespace paracobNET
{
    interface IQueriable
    {
        QueryResult Query(string path);
        QueryResult Query(string path, IDictionary<string, ulong> labels);
    }
}
