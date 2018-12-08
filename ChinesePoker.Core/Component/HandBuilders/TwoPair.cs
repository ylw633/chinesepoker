using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class TwoPair : HandBuilderBase
  {
    public override string HandName => nameof(TwoPair);

    protected override int GetStrength(IList<Card> orderedCards)
    {
      return GetCardsStrength(orderedCards[0], orderedCards[2], orderedCards[4]);
    }

    protected override int CompareCards(IList<Card> srcCards, IList<Card> targetCards)
    {
      return CompareCards(srcCards, targetCards, 0, 2, 4);
    }

    protected override IList<Card> SortCards(IList<Card> cards)
    {
      var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().Ordinal).ToList();
      var orderedCards = rankGroup[0].OrderBy(c => c.RankingAsc).Concat(rankGroup[1].OrderBy(c => c.RankingAsc)).Concat(rankGroup[2]).ToList();
      return orderedCards;
    }

    public override bool TestIsHand(IList<Card> cards)
    {
      if (cards.Count != 5) return false;
      var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().Ordinal).ToList();
      return rankGroup.Count(g => g.Count() == 2) == 2;
    }
  }
}
