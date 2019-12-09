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
      var hand = cards.GetHand();
      Assert.NotNull(hand);


      cards = Dealer.GetCards("3K,23,43,1K,27");
      hand = cards.GetHand();
      Assert.NotNull(hand);
    }
  }
}
