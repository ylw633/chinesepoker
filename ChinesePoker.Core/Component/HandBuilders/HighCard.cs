using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class HighCard : HandBuilderBase
  { 
    public override string HandName => nameof(HighCard);

    //protected override int CompareCards(IList<Card> srcCards, IList<Card> targetCards)
    //{
    //  var firstResult = CompareCards(srcCards, targetCards, 0, 1, 2);
    //  if (firstResult != 0) return firstResult;

    //  return srcCards.Count != targetCards.Count ? 0 : CompareCards(srcCards, targetCards, 3, 4);
    //}

    //protected override int GetStrength(IList<Card> orderedCards)
    //{
    //  return GetCardsStrength(Pad2Cards(orderedCards, orderedCards.Take(3).ToArray()).ToArray());
    //}

    public override bool TestIsHand(IList<Card> cards)
    {
      return true; // special case, this is acting as a fall-back
    }

    public override IEnumerable<string> GetAllPossibleComboSorted()
    {
      return GenerateAllCombo().OrderBy(c => c[0]).ThenBy(c => c[1]).ThenBy(c => c[2]).ThenBy(c => c[3]).ThenBy(c => c[4]);
    }

    protected virtual IEnumerable<string> GenerateAllCombo()
    {
      for (int i = 1; i < 10; i++)
      for (int j = i + 1; j < 11; j++)
      for (int k = j + 1; k < 12; k++)
      for (int l = k + 1; l < 13; l++)
      for (int m = l + 1; m < 14; m++)
      {
        if (j == i + 1 && k == j + 1 && l == k + 1 && m == l + 1) continue;
        if (i == 1 && j == 10 && k == 11 && l == 12 && m == 13) continue;
            
        yield return StrengthStrategy.SortByRankDesc($"{Card.OrdinalToRank(i)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(k)}{Card.OrdinalToRank(l)}{Card.OrdinalToRank(m)}");
      }
    }
  }

  public class HighCard3 : HighCard
  {
    public override IEnumerable<string> GetAllPossibleComboSorted()
    {
      return GenerateAllCombo().OrderBy(c => c[0]).ThenBy(c => c[1]).ThenBy(c => c[2]);
    }

    protected override IEnumerable<string> GenerateAllCombo()
    {
      for (int k = 1; k < 12; k++)
      for (int l = k + 1; l < 13; l++)
      for (int m = l + 1; m < 14; m++)
      {           
        yield return StrengthStrategy.SortByRankDesc($"{Card.OrdinalToRank(k)}{Card.OrdinalToRank(l)}{Card.OrdinalToRank(m)}");
      }
    }
  }
}
