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
    public IStrengthStrategy StrengthStrategy { get; } = new PokerStrengthStrategy();

    public Hand DetermineHand(IEnumerable<Card> cards, Hand maxHand = null)
    {
      var cardList = cards.OrderBy(c => c.Ordinal).ToList();
      var hand = StrengthStrategy.GetAHand(cardList);

      return hand == null || (maxHand != null && StrengthStrategy.CompareHands(hand, maxHand) >= 0) ? null : hand;
    }
  }
}
