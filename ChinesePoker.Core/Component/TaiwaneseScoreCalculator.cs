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
    public int ThreeOfKindInFirstRoundBonus { get; set; } = 3;
    public int FourOfKindInMiddleRoundBonus { get; set; } = 0;
    public int FourOfKindInLastRoundBonus { get; set; } = 4;
    public int StraightFlushInMiddleRoundBonus { get; set; } = 5;
    public int StraightFlushInLastRoundBonus { get; set; } = 5;
    public int DragonBonus { get; set; } = 36;
    public int HomeRunBonus { get; set; } = 6;
    public int GetScore(Round roundA, Round roundB, out int scoreA, out int scoreB)
    {
      scoreA = scoreB = 0;
      var strike = 0;

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
          strike = 1;
        }
        else if (pts == -3)
        {
          pts -= StrikeBonus;
          strike = -1;
        }

        pts += SpecialHandBonus(roundA) - SpecialHandBonus(roundB);
        
      }

      scoreA += pts;
      scoreB -= pts;

      return strike;
    }

    private class PlayerScore
    {
      public int Score { get; set; }
      public int StrikeCount { get; set; }
    }

    public Dictionary<Round, int> GetScores(IList<Round> rounds)
    {
      //var result = rounds.ToDictionary<Round, (int score, int strike)>(r => (0, 0));
      var result = rounds.ToDictionary(r => r, r => new PlayerScore());
      foreach (var match in new Combinations<Round>(rounds.ToList(), 2, GenerateOption.WithoutRepetition))
      {
        var strike = GetScore(match[0], match[1], out var scoreA, out var scoreB);
        result[match[0]].Score += scoreA;
        result[match[0]].StrikeCount += strike;
        result[match[1]].Score += scoreB;
        result[match[0]].StrikeCount -= strike;
      }

      // home run case
      var slugger = result.FirstOrDefault(kv => kv.Value.StrikeCount == 3);
      if (slugger.Value != null)
      {
        slugger.Value.Score += HomeRunBonus * 3;
        foreach (var otherPlayer in result.Except(new [] {slugger}))
          otherPlayer.Value.Score -= HomeRunBonus;
      }

      return result.ToDictionary(r => r.Key, r => r.Value.Score);
    }

    protected int SpecialHandBonus(Round round)
    {
      return new[]
      {
        GetScore(0, nameof(HandTypes.ThreeOfAKind), ThreeOfKindInFirstRoundBonus),
        GetScore(1, nameof(HandTypes.FourOfAKind), FourOfKindInMiddleRoundBonus),
        GetScore(1, nameof(HandTypes.StraightFlush), StraightFlushInMiddleRoundBonus),
        GetScore(2, nameof(HandTypes.FourOfAKind), FourOfKindInLastRoundBonus),
        GetScore(2, nameof(HandTypes.StraightFlush), StraightFlushInLastRoundBonus),
      }.Sum();

      int GetScore(int roundIdx, string handName, int bonus)
      {
        return round.Hands[roundIdx].Name == handName ? bonus : 0;
      }
    }
  }
}
