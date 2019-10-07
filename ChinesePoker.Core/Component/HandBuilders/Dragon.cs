using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class Dragon : HandBuilderBase
  {
    public override string HandName => nameof(Dragon);

    public override bool TestIsHand(IList<Card> cards)
    {
      if (cards.Count != 13) return false;
      var cardsList = cards.OrderBy(c => c.Ordinal).ToList();
      var ordinal = "A23456789TJQK";
      var cardRank = cardsList.Aggregate("", (s, c) => s + c.Rank);

      return cardRank == ordinal;
    }

    public override IEnumerable<string> GetAllPossibleComboSorted()
    {
      yield return "A23456789TJQK";
    }
  }
}
