using System;
using System.Text;

namespace OkayCloudSearch.Query
{
    public static class IntegerRange
    {
        public static string GetInterval(int? from, int? to)
        {
            if (from == null && to == null)
                throw new Exception("Can't build interval");

            StringBuilder s = new StringBuilder();
            if (from != null)
                s.Append(from);

            s.Append("..");

            if (to != null)
                s.Append(to);

            return s.ToString();
        }

    }
}