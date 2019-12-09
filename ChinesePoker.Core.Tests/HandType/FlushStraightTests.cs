using ChinesePoker.Core.Component.HandBuilders;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Xunit;

namespace ChinesePoker.Core.Tests.HandType
{
  public class FlushStraightTests
  {
    [Fact]
    public void should_correctly_identify_one()
    {
      var cards = Dealer.GetCards("23,25,26,22,24");
      var hand = cards.GetHand();
      Assert.NotNull(hand);
      hand.Cards.TestCardsAreInOrder("22,23,24,25,26");
    }

    [Fact]
    public void does_not_mistaken_straight_flush_with_just_flush()
    {
      var cards = Dealer.GetCards("13,35,46,12,24");
      var sf = cards.GetHand();
      Assert.NotEqual($"{nameof(HandTypes.StraightFlush)}", sf.Name);
    }
  }
}