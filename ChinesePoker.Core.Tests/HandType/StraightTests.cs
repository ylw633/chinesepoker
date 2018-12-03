using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Component.HandBuilders;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Xunit;

namespace ChinesePoker.Core.Tests.HandType
{
  public class StraightTests
  {
    [Fact]
    public void standard_straight_is_correctly_identified_and_ordered()
    {
      var cards = Dealer.GetCards("11,25,32,44,33");
      var straight = new Straight();
      var hand = straight.GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(Hand.GetHandTypeBaseStrength(HandTypes.Straight) + 9, hand.Strength);
      TestUtils.TestCardsAreInOrder(hand.Cards, "11,25,44,33,32");

      cards = Dealer.GetCards("21,313,410,111,412");
      hand = straight.GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(Hand.GetHandTypeBaseStrength(HandTypes.Straight) + 10, hand.Strength);
      TestUtils.TestCardsAreInOrder(hand.Cards, "21,313,412,111,410");

      cards = Dealer.GetCards("36,24,15,22,43");
      hand = straight.GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(Hand.GetHandTypeBaseStrength(HandTypes.Straight), hand.Strength);
      TestUtils.TestCardsAreInOrder(hand.Cards, "36,15,24,43,22");
    }
  }
}
