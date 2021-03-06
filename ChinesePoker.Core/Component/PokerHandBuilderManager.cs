﻿using System;
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
    public IHandStrengthArbiter StrengthArbiter { get; } = new PokerStrengthStrategy();

    public Hand DetermineHand(IEnumerable<Card> cards, Hand maxHand = null)
    {
      var cardList = cards.OrderBy(c => c.Ordinal).ToList();
      var hand = StrengthArbiter.GetAHand(cardList);

      return hand == null || (maxHand != null && StrengthArbiter.CompareHands(hand, maxHand) >= 0) ? null : hand;
    }
  }
}
