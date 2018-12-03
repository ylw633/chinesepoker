using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class HighCard : HandBuilderBase
  { 
    public override string HandName => nameof(HighCard);

    public override int CompareHands(IList<Card> srcCards, IList<Card> targetCards)
    {
      var firstResult = CompareCards(srcCards, targetCards, 0, 1, 2);
      if (firstResult != 0) return firstResult;

      return srcCards.Count != targetCards.Count ? 0 : CompareCards(srcCards, targetCards, 3, 4);
    }

    public override int GetStrength(IList<Card> orderedCards)
    {
      return GetCardsStrength(Pad2Cards(orderedCards, orderedCards.Take(3).ToArray()).ToArray());
    }

    public override bool TestIsHand(IList<Card> cards)
    {
      return true; // special case, this is acting as a fall-back
    }
  }
}
