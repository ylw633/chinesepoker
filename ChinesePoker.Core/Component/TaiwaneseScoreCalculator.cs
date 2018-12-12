using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Component.HandBuilders;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Combinatorics.Collections;

namespace ChinesePoker.Core.Component
{
  public class TaiwaneseScoreCalculator : IScoreCalculator
  {
    public int StrikeBonus { get; set; } = 3;
    public int ThreeOfKindInFirstRoundBonus { get; set; } = 1;
    public int FourOfKindInMiddleRoundBonus { get; set; } = 3;
    public int FourOfKindInLastRoundBonus { get; set; } = 2;
    public int StraightFlushInMiddleRoundBonus { get; set; } = 4;
    public int StraightFlushInLastRoundBonus { get; set; } = 3;
    public int DragonBonus { get; set; } = 36;
    public void GetScore(Round roundA, Round roundB, out int scoreA, out int scoreB)
    {
      scoreA = scoreB = 0;

      var pts = 0;

      if (roundA.Hands.Count == 1)
        pts += DragonBonus;
      else if (roundB.Hands.Count == 1)
        pts -= DragonBonus;
      else
      {
        for (int i = 0; i < 3; i++)
        {
          pts += roundA.Hands[i].Strength.CompareTo(roundB.Hands[i].Strength);
        }

        if (pts == 3)
        {
          pts += StrikeBonus;
        }
        else if (pts == -3)
        {
          pts -= StrikeBonus;
        }

        pts += SpecialHandBonus(roundA) - SpecialHandBonus(roundB);
        
      }

      scoreA += pts;
      scoreB -= pts;
    }

    public Dictionary<Round, int> GetScore(IList<Round> rounds)
    {
      var result = rounds.ToDictionary(r => r, r => 0);
      foreach (var match in new Combinations<Round>(rounds.ToList(), 2, GenerateOption.WithoutRepetition))
      {
        GetScore(match[0], match[1], out var scoreA, out var scoreB);
        result[match[0]] += scoreA;
        result[match[1]] += scoreB;
      }

      return result;
    }

    protected int SpecialHandBonus(Round round)
    {
      return new[]
      {
        GetScore(0, nameof(ThreeOfAKind), ThreeOfKindInFirstRoundBonus),
        GetScore(1, nameof(FourOfAKind), FourOfKindInMiddleRoundBonus),
        GetScore(1, nameof(StraightFlush), StraightFlushInMiddleRoundBonus),
        GetScore(2, nameof(FourOfAKind), FourOfKindInLastRoundBonus),
        GetScore(2, nameof(StraightFlush), StraightFlushInLastRoundBonus),
      }.Sum();

      int GetScore(int roundIdx, string handName, int bonus)
      {
        return round.Hands[roundIdx].Name == handName ? bonus : 0;
      }
    }
  }
}
