using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Helper;

namespace ChinesePoker.Core.Model
{
  public static class Dealer
  {
    public static readonly IReadOnlyList<Card> FullDeck;

    static Dealer()
    {
      var fullDeck = new Card[52];
      foreach (SuitTypes suit in Enum.GetValues(typeof(SuitTypes)))
        for (var i = 0; i < 13; i++)
          fullDeck[((int)suit - 1) * 13 + i] = new Card(suit, Card.VALID_RANKS[i]);
      FullDeck = fullDeck.ToList();
    }
    public static IList<Card> GetCards(string cardTxt)
    {
      return cardTxt.Split(',', ' ').Select(s => new Card(s)).ToList();
    }

    public static IEnumerable<IEnumerable<Card>> Deal()
    {
      var rnd = new Random();
      return FullDeck.OrderBy(c => rnd.Next()).Chunk(13);
    }

    public static IEnumerable<Card> DealOneSet()
    {
      return Deal().First();
    }
  }
}
