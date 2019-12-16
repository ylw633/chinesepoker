using ChinesePoker.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinesePoker.Core.Model
{
  public enum HandTypes
  {
    HighCard,
    OnePair,
    TwoPairs,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
    Dragon
  }

  public class Hand
  {
    public string Name { get; set; }
    public IList<Card> Cards { get; set; }
    public int Strength { get; set; }

    public Hand(string type, IList<Card> cards, int innerStrength)
    {
      Name = type;
      Cards = cards;
      Strength = innerStrength;
    }

    public override string ToString()
    {
      return $"{Name,-13} {Strength,-6:0} {string.Join(" ", Cards)}";
    }
  }
}
