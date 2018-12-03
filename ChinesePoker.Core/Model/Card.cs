using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinesePoker.Core.Model
{
  public enum SuitTypes
  {
    Spade = 1,
    Heart,
    Diamond,
    Club
  }

  public class Card
  {
    public const string VALID_RANKS = "A23456789TJQK";

    private SuitTypes _suit;

    public SuitTypes Suit
    {
      get { return _suit; }
      private set
      {
        _suit = value;
        CalibrateRanking();
      }
    }

    private char _rank;
    public char Rank
    {
      get { return _rank; }
      private set
      {
        var rank = char.ToUpper(value);
        if (!VALID_RANKS.Contains(rank)) throw new Exception($"Invalid rank: {rank}");

        _rank = rank;
        CalibrateRanking();

        switch (rank)
        {
          case 'A':
            Ordinal = 1;
            break;
          case 'K':
            Ordinal = 13;
            break;
          case 'Q':
            Ordinal = 12;
            break;
          case 'J':
            Ordinal = 11;
            break;
          case 'T':
            Ordinal = 10;
            break;
          default:
            Ordinal = int.Parse(Rank.ToString());
            break;
        }
      }
    }

    public int Ordinal { get; private set; }

    private void CalibrateRanking()
    {
      if (!VALID_RANKS.Contains(Rank)) return;

      int rankVal = 0;
      switch (Rank)
      {
        case 'A':
          rankVal = 0;
          break;
        case 'K':
          rankVal = 1;
          break;
        case 'Q':
          rankVal = 2;
          break;
        case 'J':
          rankVal = 3;
          break;
        case 'T':
          rankVal = 4;
          break;
        default:
          rankVal = 14 - int.Parse(Rank.ToString());
          break;
      }

      RankingAsc = rankVal * 4 + (int) Suit;
    }

    public int RankingAsc { get; private set; }
    public int RankingDsc => 53 - RankingAsc;

    public Card(SuitTypes suit, char rank)
    {
      Suit = suit;
      Rank = rank;
    }

    public Card(SuitTypes suit, string rank)
    {
      ParseCardText(suit.ToString()[0] + rank);
    }

    public Card(string cardText)
    {
      ParseCardText(cardText);
    }

    private void ParseCardText(string cardText)
    {
      if (string.IsNullOrWhiteSpace(cardText) || cardText.Length > 3) throw new Exception("Invalid card text");
      cardText = cardText.ToUpper();
      var suit = cardText[0];
      switch (suit)
      {
        case '1':
        case 'S':
          Suit = SuitTypes.Spade;
          break;

        case '2':
        case 'H':
          Suit = SuitTypes.Heart;
          break;

        case '3':
        case 'D':
          Suit = SuitTypes.Diamond;
          break;

        case '4':
        case 'C':
          Suit = SuitTypes.Club;
          break;

        default:
          throw new Exception($"Invalid suit: {suit}");
      }

      var rank = cardText.Substring(1).Trim();
      if (string.IsNullOrEmpty(rank) || rank.Length > 2) throw new Exception($"Invalid rank: {rank}");

      if (int.TryParse(rank, out var r))
      {
        switch (r)
        {
          case 1: rank = "A"; break;
          case 10: rank = "T"; break;
          case 11: rank = "J"; break;
          case 12: rank = "Q"; break;
          case 13: rank = "K"; break;
          case int t when t > 1 && t < 10: break;
          default: throw new Exception($"Invalid rank: {rank}");
        }
      }

      Rank = rank[0];
    }

    public override bool Equals(object obj)
    {
      var otherCard = obj as Card;
      return otherCard != null && this.Suit == otherCard.Suit && this.Rank == otherCard.Rank;
    }

    public override int GetHashCode()
    {
      return this.RankingAsc;
    }

    public override string ToString()
    {
      return Suit.ToString()[0] + Rank.ToString();
    }
  }
}
