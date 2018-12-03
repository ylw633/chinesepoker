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
  public class TwoPairTests
  {
    [Fact]
    public void two_pair_is_recognized_and_sorted_and_give_correct_strength()
    {
      var cards = Dealer.GetCards("S7,H8,C10,C8,D7");
      var hand = new TwoPair().GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(HandTypes.TwoPair, hand.HandType);
      Assert.Equal(new Card("H8"), hand.Cards[0]);
      Assert.Equal(new Card("C8"), hand.Cards[1]);
      Assert.Equal(new Card("S7"), hand.Cards[2]);
      Assert.Equal(new Card("D7"), hand.Cards[3]);
      Assert.Equal(new Card("C10"), hand.Cards[4]);

      cards = Dealer.GetCards("3K,23,43,1K,27");
      hand = new TwoPair().GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(HandTypes.TwoPair, hand.HandType);
      Assert.Equal(new Card("1K"), hand.Cards[0]);
      Assert.Equal(new Card("3K"), hand.Cards[1]);
      Assert.Equal(new Card("23"), hand.Cards[2]);
      Assert.Equal(new Card("43"), hand.Cards[3]);
      Assert.Equal(new Card("27"), hand.Cards[4]);
    }
  }
}
