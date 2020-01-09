using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;
using Microsoft.ML.Data;

namespace ChinesePoker.ML.Model
{
  public class RoundData
  {
    [LoadColumn(0)]
    public int Score { get; set; }
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

    public RoundData()
    {
    }

    public RoundData(Round round)
    {
      FirstHandStrength = round.Hands[0].Strength;
      FirstHandType = round.Hands[0].Name;

      if (round.Hands.Count < 3)
      {
        MiddleHandStrength = round.Hands[1].Strength;
        MiddleHandType = round.Hands[1].Name;
        LastHandStrength = round.Hands[2].Strength;
        LastHandType = round.Hands[2].Name;
      }
      else
      {
        MiddleHandStrength = LastHandStrength = 0;
        MiddleHandType = LastHandType = round.Hands[0].Name;
      }
    }
  }
}
