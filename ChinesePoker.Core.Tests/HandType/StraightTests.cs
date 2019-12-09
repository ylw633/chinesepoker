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
      var hand = cards.GetHand();
      Assert.NotNull(hand);
      hand.Cards.TestCardsAreInOrder("11,32,33,44,25");

      cards = Dealer.GetCards("21,313,410,111,412");
      hand = cards.GetHand();
      Assert.NotNull(hand);

      cards = Dealer.GetCards("36,24,15,22,43");
      hand = cards.GetHand();
      Assert.NotNull(hand);
      hand.Cards.TestCardsAreInOrder("22,43,24,15,36");
    }
  }
}
