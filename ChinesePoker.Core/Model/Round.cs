using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinesePoker.Core.Model
{
  public class Round
  {
    public IList<Hand> Hands { get; private set; }
    public int TotalStrength { get; private set; }

    public Round(IList<Hand> hands)
    {
      Hands = hands;
      TotalStrength = hands.Sum(h => h.Strength);
    }

    public override string ToString()
    {
      return $"{TotalStrength}\n{string.Join("\n", Hands)}";
    }
  }
}
