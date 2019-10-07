using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class OnePair : HandBuilderBase
  {
    public override string HandName => nameof(OnePair);

    //protected override int GetStrength(IList<Card> orderedCards)
    //{
    //  return GetCardsStrength(Pad2Cards(orderedCards, orderedCards[0], orderedCards[2]).ToArray());
    //}

    //protected override int CompareCards(IList<Card> srcCards, IList<Card> targetCards)
    //{
    //  var firstResult = CompareCards(srcCards, targetCards, 0, 2);
    //  if (firstResult != 0) return firstResult;

    //  return srcCards.Count != targetCards.Count ? 0 : CompareCards(srcCards, targetCards, 3, 4);
    //}

    //protected override IList<Card> SortCards(IList<Card> cards)
    //{
    //  var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().Ordinal).ToList();
    //  var orderedCards = rankGroup[0].OrderBy(c => c.RankingAsc).Concat(rankGroup.Skip(1).SelectMany(g => g).OrderBy(c => c.RankingAsc)).ToList();
    //  return orderedCards;
    //}

    public override bool TestIsHand(IList<Card> cards)
    {
      if (cards.Count != 3 && cards.Count != 5) return false;
      var rankGroup = cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().Ordinal).ToList();
      return rankGroup.Count(g => g.Count() == 2) == 1;
    }

    public override IEnumerable<string> GetAllPossibleComboSorted()
    {
      return GenerateAllCombo().OrderBy(c => c[0]).ThenBy(c => c[2]).ThenBy(c => c[3]).ThenBy(c => c[4]);
    }

    protected virtual IEnumerable<string> GenerateAllCombo()
    {
      for (int j = 1; j < 14; j++)
      for (int k = 1; k < 12; k++)
      for (int l = k + 1; l < 13; l++)
      for (int m = l + 1; m < 14; m++)
      {
        if (j == k || l == j || m == j) continue;
        yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}" + StrengthStrategy.SortByRankDesc($"{Card.OrdinalToRank(k)}{Card.OrdinalToRank(l)}{Card.OrdinalToRank(m)}");
      }
    }
  }

  public class OnePair3 : OnePair
  {
    public override IEnumerable<string> GetAllPossibleComboSorted()
    {
      return GenerateAllCombo().OrderBy(c => c[0]).ThenBy(c => c[2]);
    }

    protected override IEnumerable<string> GenerateAllCombo()
    {
      for (int j = 1; j < 14; j++)
      for (int k = 1; k < 14; k++)
      {
        if (j == k) continue;
        yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(k)}";
      }
    }
  }
}
