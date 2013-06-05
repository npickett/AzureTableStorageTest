using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public static class Extensions
    {
        public static IEnumerable<IList<T>> GroupAndSlice<T>(this IEnumerable<T> source, int groupSize, Func<T, T, bool> groupComparer)
        {
            if (source == null || groupSize == 0)
                yield break;

            var wholeGroup = new List<T>();
            var singleGroup = new List<T>();
            var backlog = new Queue<IList<T>>();

            T first = default(T);
            bool allSame = true;
            int count = 0;

            foreach (T item in source)
            {
                if (count == 0)
                    first = item;

                if (allSame && !groupComparer(item, first))
                {
                    while (backlog.Any())
                        yield return backlog.Dequeue();
                    allSame = false;
                }

                singleGroup.Add(item);
                if (allSame)
                    wholeGroup.Add(item);

                if (singleGroup.Count == groupSize)
                {
                    if (allSame)
                        backlog.Enqueue(singleGroup);
                    else
                        yield return singleGroup;
                    singleGroup = new List<T>();
                }
                count++;
            }

            if (count == 0)
                yield break;
            else if (allSame && count % groupSize == 0)
                yield return wholeGroup;
            else
            {
                while (backlog.Any())
                    yield return backlog.Dequeue();
                if (singleGroup.Any())
                    yield return singleGroup;
            }
        }
    }
}
