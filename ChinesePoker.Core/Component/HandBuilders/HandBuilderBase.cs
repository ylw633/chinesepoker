using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public abstract class HandBuilderBase : IHandBuilder
  {
    public abstract string HandName { get; }

    protected virtual int GetStrength(IList<Card> orderedCards)
    {
      return GetCardStrength(orderedCards[0]);
    }

    public int CompareHands(Hand srcHand, Hand targetHand)
    {
      if (srcHand == null || targetHand == null || srcHand.Name != targetHand.Name)
        throw new ArgumentException($"Cannot compare two different kinds of hands: {srcHand?.Name ?? "null"} and {targetHand?.Name ?? "null"}");

      return CompareCards(srcHand.Cards, targetHand.Cards);
    }

    protected virtual int CompareCards(IList<Card> srcCards, IList<Card> targetCard)
    {
      return CompareCards(srcCards, targetCard, 0);
    }

    protected static int CompareCards(IList<Card> srcCards, IList<Card> targetCards, params int[] cmpCrdIdx)
    {
      return cmpCrdIdx.Select(i => GetCardStrength(srcCards[i]).CompareTo(GetCardStrength(targetCards[i]))).FirstOrDefault(c => c != 0);
    }

    protected virtual IList<Card> SortCards(IList<Card> cards)
    {
      return cards.OrderBy(c => c.RankingAsc).ToList();
    }

    public Hand GetHand(IList<Card> cards, int handStrengthOffset = 0)
    {
      if (!TestIsHand(cards)) return null;
      var orderedCards = SortCards(cards);
      return new Hand(HandName, orderedCards, GetStrength(orderedCards) + handStrengthOffset);
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
