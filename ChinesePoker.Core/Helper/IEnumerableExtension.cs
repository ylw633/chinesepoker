using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinesePoker.Core.Helper
{
  public static class IEnumerableExtension
  {
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
    {
      while (source.Any())
      {
        yield return source.Take(chunksize);
        source = source.Skip(chunksize);
      }
    }
  }
}
