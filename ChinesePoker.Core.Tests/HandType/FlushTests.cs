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
    public void should_correctly_identify_flush()
    {
      var cards = Dealer.GetCards("47,48,4K,410,42");
      var hand = cards.GetHand();
      Assert.NotNull(hand);
    }
  }
}
