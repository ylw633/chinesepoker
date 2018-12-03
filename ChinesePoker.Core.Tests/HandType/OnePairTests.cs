using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Component.HandBuilders;
using ChinesePoker.Core.Model;
using Xunit;
using ChinesePoker.Core.Interface;

namespace ChinesePoker.Core.Tests.HandType
{
  public class OnePairTests
  {
    [Fact]
    public void high_cards_should_not_get_one_pair()
    {
      var hand = new OnePair().GetAHand(Dealer.GetCards("1A,22,33,44,45"));
      Assert.Null(hand);
    }

    [Fact]
    public void one_pair_is_recognized_and_sorted_and_give_correct_strength()
    {
      var cards = Dealer.GetCards("S7,H8,C10,C8,D4");
      var hand = new OnePair().GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(HandTypes.OnePair, hand.HandType);
      TestUtils.TestCardsAreInOrder(hand.Cards, "H8,C8,C10,S7,D4");
    }

    [Fact]
    public void one_pair_is_recognized_and_sorted_and_give_correct_strength_for_only_3_cards()
    {
      var cards = Dealer.GetCards("S7,H8,C8");
      var hand = new OnePair().GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(HandTypes.OnePair, hand.HandType);
      TestUtils.TestCardsAreInOrder(hand.Cards, "H8,C8,S7");
    }

    [Fact]
    public void two_pairs_is_not_one_pair()
    {
      var cards = Dealer.GetCards("SA,HA,Sk,HQ,CQ");
      Assert.Null(new OnePair().GetAHand(cards));
    }
  }
}
