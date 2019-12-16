using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Component.HandBuilders;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Xunit;

namespace ChinesePoker.Core.Tests
{
  public static class TestUtils
  {
    public static void TestCardsAreInOrder(this IList<Card> cards, string supposedOrder)
    {
      Assert.NotNull(cards);
      var expectedCards = Dealer.GetCards(supposedOrder);
      Assert.NotNull(supposedOrder);
      Assert.Equal(expectedCards.Count, cards.Count);
      for (var i = 0; i < expectedCards.Count; i++)
        Assert.Equal(expectedCards[i], cards[i]);
    }

    public static Hand GetHand(this IList<Card> cards)
    {
      return new PokerStrengthStrategy().GetAHand(cards);
    }

    public static int CompareHand(this Hand handA, Hand handB)
    {
      return new PokerStrengthStrategy().CompareHands(handA, handB);
    }
  }
}
