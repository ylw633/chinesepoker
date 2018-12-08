using System;
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
  public class FourOfAKindTests
  {
    [Fact]
    public void should_correctly_identify_4_of_a_kind()
    {
      var cards = Dealer.GetCards("25,32,42,12,22");
      var hand = cards.GetHand<FourOfAKind>();
      Assert.NotNull(hand);
      hand.Cards.TestCardsAreInOrder("12,22,32,42,25");
    }
  }
}
