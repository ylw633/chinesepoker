using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class ThreeOfAKind : HandBuilderBase
  {
    public override string HandName => nameof(ThreeOfAKind);

    protected override IList<Card> SortCards(IList<Card> cards)
    {
      var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().Ordinal).ToList();
      var orderedCards = rankGroup[0].OrderBy(c => c.RankingAsc).Concat(rankGroup.Skip(1).SelectMany(g => g).OrderBy(c => c.RankingAsc)).ToList();
      return orderedCards;
    }

    public override bool TestIsHand(IList<Card> cards)
    {
      if (cards.Count != 3 && cards.Count != 5) return false;
      var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().Ordinal).ToList();
      return rankGroup.Count(g => g.Count() == 3) == 1 && (rankGroup.Count <= 1 || rankGroup[1].Count() != 2);
    }
  }
}
