using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class Straight : HandBuilderBase
  {
    private static readonly string[] _possibleForm = {"A23456789TJQK", "ATJQK" };

    public override string HandName => nameof(Straight);

    protected override int CompareCards(IList<Card> srcCards, IList<Card> targetCards)
    {
      return CompareCards(srcCards, targetCards, 4);
    }

    protected override int GetStrength(IList<Card> orderedCards)
    {
      return GetCardStrength(orderedCards[4]);
    }

    protected override IList<Card> SortCards(IList<Card> cards)
    {
      var sortedCards = cards.OrderBy(c => c.Ordinal).ToList();
      var cardRank = new string(sortedCards.Select(c => c.Rank).ToArray());
      if (string.Compare(cardRank, "ATJQK", StringComparison.CurrentCultureIgnoreCase) == 0)
      {
        var ace = sortedCards[0];
        sortedCards.RemoveAt(0);
        sortedCards.Add(ace);
      }

      return sortedCards;
    }

    public override bool TestIsHand(IList<Card> cards)
    {
      if (cards.Count != 5) return false;

      var cardRank = new string(cards.OrderBy(c => c.Ordinal).Select(c => c.Rank).ToArray());
      return _possibleForm.Any(form => form.IndexOf(cardRank, StringComparison.OrdinalIgnoreCase) > -1);
    }
  }
}
