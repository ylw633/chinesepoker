using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class OnePair : HandBuilderBase
  {
    public override string HandName => nameof(OnePair);

    public override int GetStrength(IList<Card> orderedCards)
    {
      return GetCardsStrength(Pad2Cards(orderedCards, orderedCards[0], orderedCards[2]).ToArray());
    }

    public override int CompareHands(IList<Card> srcCards, IList<Card> targetCards)
    {
      var firstResult = CompareCards(srcCards, targetCards, 0, 2);
      if (firstResult != 0) return firstResult;

      return srcCards.Count != targetCards.Count ? 0 : CompareCards(srcCards, targetCards, 3, 4);
    }

    public override IList<Card> SortCards(IList<Card> cards)
    {
      var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().Ordinal).ToList();
      var orderedCards = rankGroup[0].OrderBy(c => c.RankingAsc).Concat(rankGroup.Skip(1).SelectMany(g => g).OrderBy(c => c.RankingAsc)).ToList();
      return orderedCards;
    }

    public override bool TestIsHand(IList<Card> cards)
    {
      if (cards.Count != 3 && cards.Count != 5) return false;
      var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().Ordinal).ToList();
      return rankGroup.Count(g => g.Count() == 2) == 1;
    }
  }
}
