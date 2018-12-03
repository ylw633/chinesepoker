using System;
using System.Collections.Generic;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component.HandBuilders
{
  public class StraightFlush : Straight
  {
    public override string HandName => nameof(StraightFlush);

    public override bool TestIsHand(IList<Card> cards)
    {
      return base.TestIsHand(cards) && (new Flush()).TestIsHand(cards);
    }
  }
}
