using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;
using Microsoft.ML.Data;

namespace ChinesePoker.ML.Model
{
  public class RoundData<T> where T: struct
  {
    [LoadColumn(0)]
    public T Score { get; set; }

    [LoadColumn(1)]
    public float FirstHandStrength { get; set; }
    [LoadColumn(2)]
    public float MiddleHandStrength { get; set; }
    [LoadColumn(3)]
    public float LastHandStrength { get; set; }

    [LoadColumn(4)]
    public string FirstHandType { get; set; }
    [LoadColumn(5)]
    public string MiddleHandType { get; set; }
    [LoadColumn(6)]
    public string LastHandType { get; set; }

    [LoadColumn(7)]
    public string Card1 { get; set; }
    [LoadColumn(8)]
    public string Card2 { get; set; }
    [LoadColumn(9)]
    public string Card3 { get; set; }
    [LoadColumn(10)]
    public string Card4 { get; set; }
    [LoadColumn(11)]
    public string Card5 { get; set; }
    [LoadColumn(12)]
    public string Card6 { get; set; }
    [LoadColumn(13)]
    public string Card7 { get; set; }
    [LoadColumn(14)]
    public string Card8 { get; set; }
    [LoadColumn(15)]
    public string Card9 { get; set; }
    [LoadColumn(16)]
    public string Card10 { get; set; }
    [LoadColumn(17)]
    public string Card11 { get; set; }
    [LoadColumn(18)]
    public string Card12 { get; set; }
    [LoadColumn(19)]
    public string Card13 { get; set; }

    [LoadColumn(20)]
    public int PlayerIndex { get; set; }
    [LoadColumn(21)]
    public int Player1RoundIndex { get; set; }
    [LoadColumn(22)]
    public int Player2RoundIndex { get; set; }
    [LoadColumn(23)]
    public int Player3RoundIndex { get; set; }
    [LoadColumn(24)]
    public int Player4RoundIndex { get; set; }

    public RoundData()
    {
    }

    public RoundData(Round round, int player1Index = 0)
    {
      FirstHandStrength = round.Hands[0].Strength;
      FirstHandType = round.Hands[0].Name;
      Card1 = round.Hands[0].Cards[0].ToString();
      Card2 = round.Hands[0].Cards[1].ToString();
      Card3 = round.Hands[0].Cards[2].ToString();

      if (round.Hands.Count >= 3)
      {
        MiddleHandStrength = round.Hands[1].Strength;
        MiddleHandType = round.Hands[1].Name;
        LastHandStrength = round.Hands[2].Strength;
        LastHandType = round.Hands[2].Name;
        Card4 = round.Hands[1].Cards[0].ToString();
        Card5 = round.Hands[1].Cards[1].ToString();
        Card6 = round.Hands[1].Cards[2].ToString();
        Card7 = round.Hands[1].Cards[3].ToString();
        Card8 = round.Hands[1].Cards[4].ToString();
        Card9 = round.Hands[2].Cards[0].ToString();
        Card10 = round.Hands[2].Cards[1].ToString();
        Card11 = round.Hands[2].Cards[2].ToString();
        Card12 = round.Hands[2].Cards[3].ToString();
        Card13 = round.Hands[2].Cards[4].ToString();
      }
      else
      {
        MiddleHandStrength = LastHandStrength = 0;
        MiddleHandType = LastHandType = round.Hands[0].Name;
        Card4 = round.Hands[0].Cards[3].ToString();
        Card5 = round.Hands[0].Cards[4].ToString();
        Card6 = round.Hands[0].Cards[5].ToString();
        Card7 = round.Hands[0].Cards[6].ToString();
        Card8 = round.Hands[0].Cards[7].ToString();
        Card9 = round.Hands[0].Cards[8].ToString();
        Card10 = round.Hands[0].Cards[9].ToString();
        Card11 = round.Hands[0].Cards[10].ToString();
        Card12 = round.Hands[0].Cards[11].ToString();
        Card13 = round.Hands[0].Cards[12].ToString();
      }

      PlayerIndex = Player2RoundIndex = Player3RoundIndex = Player4RoundIndex = 0;
      Player1RoundIndex = Math.Min(4, player1Index);
    }
  }
}
