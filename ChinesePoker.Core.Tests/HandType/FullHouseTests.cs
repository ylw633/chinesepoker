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
  public class FullHouseTests
  {
    [Fact]
    public void should_correctly_identify_a_full_house()
    {
      var cards = Dealer.GetCards("33,25,35,43,23");
      var hand = cards.GetHand();
      Assert.NotNull(hand);
      hand.Cards.TestCardsAreInOrder("23,33,43,25,35");
    }
  }
}
