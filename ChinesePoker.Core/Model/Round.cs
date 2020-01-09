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
    public int Strength { get; private set; }

    public Round(IList<Hand> hands, int strength)
    {
      Hands = hands;
      Strength = strength;
    }

    public override string ToString()
    {
      return $"{Strength:0} {string.Join(" ", Hands.Select(h => $"({h})"))}";
    }
  }
}
