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
  public class FlushTests
  {
    [Fact]
    public void should_correcty_identify_flush()
    {
      var cards = Dealer.GetCards("47,48,4K,410,42");
      var hand = new Flush().GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(HandTypes.Flush, hand.HandType);
      TestUtils.TestCardsAreInOrder(hand.Cards, "4K,410,48,47,42");
    }
  }
}
