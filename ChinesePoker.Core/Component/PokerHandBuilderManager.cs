using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Component.HandBuilders;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component
{

  public class PokerHandBuilderManager : IGameHandsManager
  {
    #region utility sub-class

    public interface IBD
    {
      IHandBuilder HandBuilder { get; }
      int BaseStrength { get; }
    } // dummy
    public class BD<T> : IBD where T : IHandBuilder, new()
    {
      public IHandBuilder HandBuilder { get; } = new T();

      private int _baseStrength = -1;

      public int BaseStrength
      {
        get
        {
          if (_baseStrength == -1 && _allHandBuilders != null)
            _baseStrength = GetHandTypeBaseStrength<T>();
          return _baseStrength;
        }
      }
    }    

    public static int GetHandTypeBaseStrength<T>(int offset = 0) where T: IHandBuilder
    {
      return _allHandBuilders.FindIndex(bd => bd.HandBuilder.GetType() == typeof(T)) * 3000000 + offset; // 3000000 ~= 12^6 (there are 13 ranks with offset 0->12, and at most 5 cards, so each level gap should be 12^6) 
    }

    #endregion

    private static readonly List<IBD> _allHandBuilders;
    private static readonly Dictionary<int, List<IBD>> _handTypeTesters;

    #region setup
    static PokerHandBuilderManager()
    {
      _allHandBuilders = new List<IBD>(new IBD[]
      {
        new BD<HighCard>(),
        new BD<OnePair>(), 
        new BD<TwoPair>(), 
        new BD<ThreeOfAKind>(),
        new BD<Straight>(),
        new BD<Flush>(), 
        new BD<FullHouse>(), 
        new BD<FourOfAKind>(), 
        new BD<StraightFlush>(), 
        new BD<Dragon>(), 
      });

      _handTypeTesters = new Dictionary<int, List<IBD>>
      {
        { 13, new List<IBD> { FindBd<Dragon>() } },
        {  5, new List<IBD>
          {
            FindBd<StraightFlush>(),
            FindBd<FourOfAKind>(), 
            FindBd<FullHouse>(), 
            FindBd<Flush>(), 
            FindBd<Straight>(), 
            FindBd<ThreeOfAKind>(), 
            FindBd<TwoPair>(), 
            FindBd<OnePair>(), 
            FindBd<HighCard>(), 
          }
        },
        {  3, new List<IBD>
          {
            FindBd<ThreeOfAKind>(), 
            FindBd<OnePair>(), 
            FindBd<HighCard>(), 
          }
        }
      };

      IBD FindBd<T>() where T : IHandBuilder
      {
        return _allHandBuilders.FirstOrDefault(bd => bd.HandBuilder.GetType() == typeof(T));
      }
    }
    #endregion


    public Hand DetermineHand(IEnumerable<Card> cards, Hand maxHand = null)
    {
      var cardList = cards.ToList();
      // see if it's 13, 5, or 3 cards
      if (!_handTypeTesters.ContainsKey(cardList.Count)) return null;

      // get possible testers according to card count
      var testers = _handTypeTesters[cardList.Count];
      foreach (var bd in testers)
      {
        if (bd.BaseStrength >= maxHand?.Strength) continue;

        var handBuilder = bd.HandBuilder;
        var hand = handBuilder.GetHand(cardList, bd.BaseStrength);

        if (hand == null) continue;
        if (maxHand != null && bd.HandBuilder.HandName == maxHand.Name && handBuilder.CompareHands(hand, maxHand) > 0) continue;
        
        return hand;
      }

      return null;
    }

    public int MinHandStrengthThreshold { get; } = _allHandBuilders[1].BaseStrength; // at least one pair
  }
}
