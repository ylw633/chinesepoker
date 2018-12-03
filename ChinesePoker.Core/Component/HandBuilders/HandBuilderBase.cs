using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public abstract class HandBuilderBase : IHandBuilder
  {
    public abstract string HandName { get; }

    public virtual int GetStrength(IList<Card> orderedCards)
    {
      return GetCardStrength(orderedCards[0]);
    }

    public virtual int CompareHands(IList<Card> srcCards, IList<Card> targetCards)
    {
      return CompareCards(srcCards, targetCards, 0);
    }

    public static int CompareCards(IList<Card> srcCards, IList<Card> targetCards, params int[] cmpCrdIdx)
    {
      return cmpCrdIdx.Select(i => GetCardStrength(srcCards[i]).CompareTo(GetCardStrength(targetCards[i]))).FirstOrDefault(c => c != 0);
    }

    public virtual IList<Card> SortCards(IList<Card> cards)
    {
      return cards.OrderBy(c => c.RankingAsc).ToList();
    }

    public abstract bool TestIsHand(IList<Card> cards);

    public static int GetCardStrength(Card card)
    {
      return card.Ordinal == 1 ? 12 : card.Ordinal - 2;
    }

    public static int GetCardsStrength(params Card[] cards)
    {
      int strength = GetCardStrength(cards[0]);
      for (int i = 1; i < cards.Length; i++)
      {
        strength = strength * 12 + GetCardStrength(cards[i]);
      }

      return strength;
    }

    public static IList<Card> Pad2Cards(IList<Card> src, params Card[] requiredCards)
    {
      var ret = new List<Card>(requiredCards);
      if (src.Count == 3)
      {
        ret.Add(new Card(SuitTypes.Club, '2'));
        ret.Add(new Card(SuitTypes.Club, '2'));
      }
      else
      {
        ret.Add(src[3]);
        ret.Add(src[4]);
      }

      return ret;
    }
  }
}
