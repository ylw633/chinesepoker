using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Helper
{
  public class CardComparer : IComparer<string>
  {
    List<Func<string, string>> GroupSelector { get; }
    public CardComparer()
    {
      GroupSelector = new List<System.Func<string, string>>() { f => f };
    }

    public CardComparer(IEnumerable<Func<string, string>> groupSelector)
    {
      GroupSelector = new List<System.Func<string, string>>(groupSelector);
    }

    public int Compare(string x, string y)
    {
      foreach (var gs in GroupSelector)
      {
        var x1 = gs(x);
        var y1 = gs(y);

        for (int i = 0; i < x1.Length && i < y1.Length; i++)
        {
          var a = FindLargest(x1, out var ai);
          var b = FindLargest(y1, out var bi);
          if (a != b) return a.CompareTo(b);

          x1 = x1.Remove(ai, 1);
          y1 = y1.Remove(bi, 1);
        }
      }

      return 0;
    }

    private int O2S(int o) { return o == 1 ? 14 : o; }
    private int FindLargest(string str, out int idx)
    {
      idx = -1;
      int maxVal = -1;
      for (int i = 0; i < str.Length; i++)
      {
        var strength = O2S(Card.RankToOrdinal(str[i]));
        if (strength > maxVal)
        {
          maxVal = strength;
          idx = i;
        }
      }

      return maxVal;
    }
  }
}
