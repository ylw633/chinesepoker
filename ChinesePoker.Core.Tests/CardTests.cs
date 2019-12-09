using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;
using Xunit;

namespace ChinesePoker.Core.Tests
{
  public class CardTests
  {
    [Fact]
    public void default_constructor_should_give_correct_card()
    {
      var card = new Card(SuitTypes.Spade, "K");
      Assert.Equal(SuitTypes.Spade, card.Suit);
      Assert.Equal('K', card.Rank);
      Assert.Equal(13, card.Ordinal);
    }

    [Fact]
    public void mixed_string_format_accepted()
    {
      var cards = Dealer.GetCards("11,25,D2,44,33,410,S10,SQ,C11,H13,SK,DQ,S12,1a");
      Assert.Equal(new Card(SuitTypes.Spade, "A"), cards[0]);
      Assert.Equal(new Card(SuitTypes.Heart, "5"), cards[1]);
      Assert.Equal(new Card(SuitTypes.Diamond, "2"), cards[2]);
      Assert.Equal(new Card(SuitTypes.Club, "4"), cards[3]);
      Assert.Equal(new Card(SuitTypes.Diamond, "3"), cards[4]);
      Assert.Equal(new Card(SuitTypes.Club, "10"), cards[5]);
      Assert.Equal(new Card(SuitTypes.Spade, "10"), cards[6]);
      Assert.Equal(new Card(SuitTypes.Spade, "Q"), cards[7]);
      Assert.Equal(new Card(SuitTypes.Club, "J"), cards[8]);
      Assert.Equal(new Card(SuitTypes.Heart, "K"), cards[9]);
      Assert.Equal(new Card(SuitTypes.Spade, "K"), cards[10]);
      Assert.Equal(new Card(SuitTypes.Diamond, "Q"), cards[11]);
      Assert.Equal(new Card(SuitTypes.Spade, "Q"), cards[12]);
      Assert.Equal(new Card(SuitTypes.Spade, "A"), cards[13]);
    }
  }
}
