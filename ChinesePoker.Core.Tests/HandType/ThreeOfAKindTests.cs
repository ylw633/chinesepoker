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
  public class ThreeOfAKindTests
  {
    [Fact]
    public void should_correctly_identify_with_5_cards()
    {
      var cards = Dealer.GetCards("H2,C8,S5,S8,D8");
      var hand = new ThreeOfAKind().GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(HandTypes.ThreeOfAKind, hand.HandType);
      TestUtils.TestCardsAreInOrder(hand.Cards, "S8,D8,C8,S5,H2");
    }

    [Fact]
    public void should_correctly_identify_with_3_cards()
    {
      var cards = Dealer.GetCards("17,27,37");
      var hand = new ThreeOfAKind().GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(HandTypes.ThreeOfAKind, hand.HandType);
      TestUtils.TestCardsAreInOrder(hand.Cards, "17,27,37");
    }

    [Fact]
    public void dont_mistaken_full_house_with_three_of_a_kind()
    {
      var cards = Dealer.GetCards("210,310,113,413,313");
      var hand = new ThreeOfAKind().GetAHand(cards);
      Assert.Null(hand);
    }
  }
}
