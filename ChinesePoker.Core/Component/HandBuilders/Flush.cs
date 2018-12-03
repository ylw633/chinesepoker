using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class Flush : HandBuilderBase
  {
    public override string HandName => nameof(Flush);

    public override bool TestIsHand(IList<Card> cards)
    {
      if (cards.Count != 5) return false;

      var firstSuit = cards[0].Suit;
      return cards.Skip(1).All(c => c.Suit == firstSuit);
    }

    public override int CompareHands(IList<Card> srcCards, IList<Card> targetCards)
    {
      return CompareCards(srcCards, targetCards, 0, 1, 2, 3, 4);
    }

    public override int GetStrength(IList<Card> orderedCards)
    {
      return GetCardsStrength(orderedCards.ToArray());
    }
  }
}
