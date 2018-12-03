using ChinesePoker.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinesePoker.Core.Model
{
  public class Hand
  {
    public string Name { get; set; }
    public IList<Card> Cards { get; set; }
    public int Strength { get; set; }

    public Hand(string name, IList<Card> cards, int innerStrength)
    {
      Name = name;
      Cards = cards;
      Strength = innerStrength;
    }

    public override string ToString()
    {
      return $"{Name} {Strength} {string.Join(" ", Cards)}";
    }
  }
}
