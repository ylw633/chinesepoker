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
    public IHandStrengthArbiter StrengthStrategy { get; set; }

    public int StrikeBonus { get; set; } = 3;
    public int ThreeOfKindInFirstRoundBonus { get; set; } = 3;
    public int FourOfKindInMiddleRoundBonus { get; set; } = 4;
    public int FourOfKindInLastRoundBonus { get; set; } = 4;
    public int StraightFlushInMiddleRoundBonus { get; set; } = 5;
    public int StraightFlushInLastRoundBonus { get; set; } = 5;
    public int DragonBonus { get; set; } = 36;
    public int HomeRunBonus { get; set; } = 6;

    public TaiwaneseScoreCalculator(IHandStrengthArbiter strengthStrategy)
    {
      StrengthStrategy = strengthStrategy;
    }

    public TaiwaneseScoreCalculator() : this(new PokerStrengthStrategy())
    {
    }

    public int GetScore(Round roundA, Round roundB, out int scoreA, out int scoreB)
    {
      GetScore(roundA, roundB, out PlayerScore psA, out var psB);
      scoreA = psA.TotalScore;
      scoreB = psB.TotalScore;

      return psA.StrikeCount > 0 ? 1 : psB.StrikeCount > 0 ? -1 : 0;
    }

    public Dictionary<Round, int> GetScores(IList<Round> rounds)
    {
      return GetScoresWithRoundWeight(rounds).ToDictionary(r => r.Key, r => r.Value.TotalScore);
    }

    public void GetScore(Round roundA, Round roundB, out PlayerScore scoreA, out PlayerScore scoreB)
    {
      scoreA = new PlayerScore();
      scoreB = new PlayerScore();

      if (roundA.Hands.Count == 1 || roundB.Hands.Count == 1)
      {
        if (roundA.Hands.Count == 1)
        {
          scoreA.TotalScore = DragonBonus;
          scoreB.TotalScore = -DragonBonus;
        }
        else
        {
          scoreA.TotalScore = -DragonBonus;
          scoreB.TotalScore = DragonBonus;
        }

        return;
      }

      var strike = 0;
      var specialHandBonus = GetSpecialHandBonus();
      for (int i = 0; i < 3; i++)
      {
        var result = StrengthStrategy.CompareHands(roundA.Hands[i], roundB.Hands[i]);
        if (result > 0)
        {
          scoreA.RoundWeight[i]++;
          strike++;
        }
        else if (result < 0)
        {
          scoreB.RoundWeight[i]++;
          strike--;
        }

        scoreA.TotalScore += result;
        scoreB.TotalScore -= result;

        SquareOffSpecialHand(i, roundA, scoreA, scoreB);
        SquareOffSpecialHand(i, roundB, scoreA, scoreB);
      }

      if (strike == 3)
      {
        SquareOffStrike(scoreA, scoreB);
      }
      else if (strike == -3)
      {
        SquareOffStrike(scoreB, scoreA);
      }

      void SquareOffSpecialHand(int roundIdx, Round targetRound, PlayerScore targetScore, PlayerScore opponentScore)
      {
        if (!specialHandBonus[roundIdx].ContainsKey(targetRound.Hands[roundIdx].Name))
          return;

        var bonus = specialHandBonus[roundIdx][targetRound.Hands[roundIdx].Name];
        targetScore.RoundWeight[roundIdx] += bonus;
        targetScore.TotalScore += bonus;
        opponentScore.TotalScore -= bonus;
      }

      void SquareOffStrike(PlayerScore strikeScore, PlayerScore opponentScore)
      {
        strikeScore.StrikeCount = 1;
        strikeScore.TotalScore += StrikeBonus;
        opponentScore.StrikeCount = -1;
        opponentScore.TotalScore -= StrikeBonus;
      }
    }

    public Dictionary<Round, PlayerScore> GetScoresWithRoundWeight(IList<Round> rounds)
    {
      var result = rounds.ToDictionary(r => r, r => new PlayerScore());
      foreach (var match in new Combinations<Round>(rounds.ToList(), 2, GenerateOption.WithoutRepetition))
      {
        GetScore(match[0], match[1], out PlayerScore scoreA, out var scoreB);
        CopyScore(result[match[0]], scoreA);
        CopyScore(result[match[1]], scoreB);
      }

      // home run case
      var slugger = result.FirstOrDefault(kv => kv.Value.StrikeCount == 3);
      if (slugger.Value != null)
      {
        slugger.Value.TotalScore += HomeRunBonus * 3;
        foreach (var otherPlayer in result.Except(new[] { slugger }))
          otherPlayer.Value.TotalScore -= HomeRunBonus;
      }

      void CopyScore(PlayerScore targetScore, PlayerScore srcScore)
      {
        targetScore.TotalScore += srcScore.TotalScore;
        targetScore.StrikeCount += srcScore.StrikeCount;
        for (int i = 0; i < 3; i++)
          targetScore.RoundWeight[i] += srcScore.RoundWeight[i];
      }

      return result;
    }

    protected Dictionary<int, Dictionary<string, int>> GetSpecialHandBonus()
    {
      return new Dictionary<int, Dictionary<string, int>>
      {
        { 0, new Dictionary<string, int>
          {
            { nameof(HandTypes.ThreeOfAKind), ThreeOfKindInFirstRoundBonus },
          }
        },
        { 1, new Dictionary<string, int>
          {
            { nameof(HandTypes.FourOfAKind), FourOfKindInMiddleRoundBonus },
            { nameof(HandTypes.StraightFlush), StraightFlushInMiddleRoundBonus }
          }
        },
        { 2, new Dictionary<string, int>
          {
            { nameof(HandTypes.FourOfAKind), FourOfKindInLastRoundBonus },
            { nameof(HandTypes.StraightFlush), StraightFlushInLastRoundBonus }
          }
        }
      };
    }
  }
}
