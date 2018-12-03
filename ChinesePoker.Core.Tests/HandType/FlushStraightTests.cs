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
      var hand = new StraightFlush().GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(HandTypes.StraightFlush, hand.HandType);
      TestUtils.TestCardsAreInOrder(hand.Cards, "26,25,24,23,22");
    }

    [Fact]
    public void does_not_mistaken_flush_with_straight_flush()
    {
      var cards = Dealer.GetCards("13,35,46,12,24");
      var hand = new StraightFlush().GetAHand(cards);
      Assert.Null(hand);
    }

    [Fact]
    public void can_identify_royal_flush()
    {
      var cards = Dealer.GetCards("2A,2K,210,2J,2q");
      var hand = new StraightFlush().GetAHand(cards);
      Assert.NotNull(hand);
      Assert.Equal(HandTypes.RoyalFlush, hand.HandType);
    }
  }
}