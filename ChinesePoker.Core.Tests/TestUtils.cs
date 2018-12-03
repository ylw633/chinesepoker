using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;
using Xunit;

namespace ChinesePoker.Core.Tests
{
  public class TestUtils
  {
    public static void TestCardsAreInOrder(IList<Card> cards, string supposedOrder)
    {
      Assert.NotNull(cards);
      var expectedCards = Dealer.GetCards(supposedOrder);
      Assert.NotNull(supposedOrder);
      Assert.Equal(expectedCards.Count, cards.Count);
      for (var i = 0; i < expectedCards.Count; i++)
        Assert.Equal(expectedCards[i], cards[i]);
    }
  }
}
