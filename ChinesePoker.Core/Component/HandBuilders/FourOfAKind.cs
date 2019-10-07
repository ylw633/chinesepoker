using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class FourOfAKind : HandBuilderBase
  {
    public override string HandName => nameof(FourOfAKind);

    //protected override IList<Card> SortCards(IList<Card> cards)
    //{
    //  var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ToList();
    //  return rankGroup[0].OrderBy(c => c.RankingAsc).Concat(rankGroup[1].OrderBy(c => c.RankingAsc)).ToList();
    //}

    public override bool TestIsHand(IList<Card> cards)
    {
      if (cards.Count != 5) return false;
      var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ToList();
      return rankGroup.Count(g => g.Count() == 4) == 1;
    }

    public override IEnumerable<string> GetAllPossibleComboSorted()
    {
      for (int j = 2; j < 14; j++)
      {
        yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}";
      }

      yield return $"{Card.OrdinalToRank(1)}{Card.OrdinalToRank(1)}{Card.OrdinalToRank(1)}{Card.OrdinalToRank(1)}";
    }
  }
}
