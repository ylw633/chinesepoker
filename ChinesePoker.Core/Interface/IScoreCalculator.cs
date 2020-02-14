using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  public interface IScoreCalculator
  {
    int GetScore(Round roundA, Round roundB, out int scoreA, out int scoreB);
    Dictionary<Round, int> GetScores(IList<Round> rounds);
    void GetScore(Round roundA, Round roundB, out PlayerScore scoreA, out PlayerScore scoreB);
    Dictionary<Round, PlayerScore> GetScoresWithRoundWeight(IList<Round> rounds);
  }

  public class PlayerScore
  {
    public int[] RoundWeight { get; } = new int[3];
    public int TotalScore { get; set; }
    public int StrikeCount { get; set; }

    public PlayerScore()
    {
      RoundWeight[0] = RoundWeight[1] = RoundWeight[2] = 1;
    }
  }
}
