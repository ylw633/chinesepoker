using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
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

    public SuitTypes Suit { get; private set; }

    private char _rank;
    public char Rank
    {
      get => _rank;
      private set
      {
        var rank = char.ToUpper(value);
        if (!VALID_RANKS.Contains(rank)) throw new Exception($"Invalid rank: {rank}");

        _rank = rank;
        Ordinal = RankToOrdinal(rank);
      }
    }

    public int Ordinal { get; private set; }

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

    public static char OrdinalToRank(int carNum)
    {
      if (carNum < 1 || carNum > 13) throw new ArgumentException($"Invalid input: {carNum} ", nameof(carNum));

      switch (carNum)
      {
        case 1: return 'A';
        case 10: return 'T';
        case 11: return 'J';
        case 12: return 'Q';
        case 13: return 'K';
        default:
          return (char)('0' + carNum);
      }
    }

    public static int RankToOrdinal(char rank)
    {
      rank = char.ToUpper(rank);
      if (!VALID_RANKS.Contains(rank)) throw new Exception($"Invalid rank: {rank}");

      switch (rank)
      {
        case 'A': return 1;
        case 'K': return 13;
        case 'Q': return 12;
        case 'J': return 11;
        case 'T': return 10;
        default: return int.Parse(rank.ToString());
      }
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
      return obj is Card otherCard && this.Suit == otherCard.Suit && this.Rank == otherCard.Rank;
    }

    protected bool Equals(Card other)
    {
      return Suit == other.Suit && _rank == other._rank && Ordinal == other.Ordinal;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (int) Suit;
        hashCode = (hashCode * 397) ^ _rank.GetHashCode();
        hashCode = (hashCode * 397) ^ Ordinal;
        return hashCode;
      }
    }

    public override string ToString()
    {
      return Suit.ToString()[0] + Rank.ToString();
    }
  }
}
