﻿using System;
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
      var hand = cards.GetHand();
      Assert.NotNull(hand);
    }

    [Fact]
    public void should_correctly_identify_with_3_cards()
    {
      var cards = Dealer.GetCards("17,27,37");
      var hand = cards.GetHand();
      Assert.NotNull(hand);
      hand.Cards.TestCardsAreInOrder("17,27,37");
    }

    [Fact]
    public void dont_mistaken_full_house_with_three_of_a_kind()
    {
      var cards = Dealer.GetCards("210,310,113,413,313");
      var hand = cards.GetHand();
      Assert.NotEqual(nameof(HandTypes.ThreeOfAKind), hand.Name);
    }

    [Fact]
    public void toak3_and_toak5_compare_should_get_correct_result()
    {
      var toak3 = Dealer.GetCards("C2 S2 D2").GetHand();
      var toak5 = Dealer.GetCards("DA D2 C2 H2 ST").GetHand();

      Assert.Equal(nameof(HandTypes.ThreeOfAKind), toak3.Name);
      Assert.Equal(nameof(HandTypes.ThreeOfAKind), toak5.Name);
      Assert.Equal(0, toak3.CompareHand(toak5));

      toak5 = Dealer.GetCards("DA CA SA HQ DK").GetHand();
      Assert.Equal(nameof(HandTypes.ThreeOfAKind), toak5.Name);
      Assert.Equal(-1, toak3.CompareHand(toak5));
    }
  }
}
