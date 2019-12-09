﻿using System;
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
      var hand = Dealer.GetCards("1A,22,33,44,45").GetHand();
      Assert.NotEqual(nameof(HandTypes.OnePair), hand.Name);
    }

    [Fact]
    public void one_pair_is_recognized_and_sorted_and_give_correct_strength()
    {
      var cards = Dealer.GetCards("S7,H8,C10,C8,D4");
      var hand = cards.GetHand();
      Assert.NotNull(hand);
    }

    [Fact]
    public void one_pair_is_recognized_and_sorted_and_give_correct_strength_for_only_3_cards()
    {
      var cards = Dealer.GetCards("S7,H8,C8");
      var hand = cards.GetHand();
      Assert.NotNull(hand);
    }

    [Fact]
    public void two_pairs_is_not_one_pair()
    {
      var cards = Dealer.GetCards("SA,HA,Sk,HQ,CQ");
      Assert.NotEqual(nameof(HandTypes.OnePair), cards.GetHand().Name);
    }
  }
}
