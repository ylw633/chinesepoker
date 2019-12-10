using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Component.HandBuilders;
using ChinesePoker.Core.Model;
using Xunit;

namespace ChinesePoker.Core.Tests
{
  public class ScoreCalculatorTests
  {
    [Fact]
    public void Two_players_should_have_scores_cancel_out()
    {
      var strategy = new SimpleRoundStrategy();
      var sets = Dealer.Deal().Take(2).ToArray();

      var roundA = strategy.GetBestRound(sets[0]);
      var roundB = strategy.GetBestRound(sets[1]);

      var calculator = new TaiwaneseScoreCalculator(new BasicStrengthStrategy());
      calculator.GetScore(roundA, roundB, out var scoreA, out var scoreB);

      Assert.Equal(scoreA, -scoreB);
      Assert.Equal(0, scoreA + scoreB);
    }

    [Fact]
    public void Four_players_should_have_scores_cancel_out()
    {
      var strategy = new SimpleRoundStrategy();
      var sets = Dealer.Deal().ToArray();
      
      var calculator = new TaiwaneseScoreCalculator(new BasicStrengthStrategy());
      var scores = calculator.GetScores(sets.Select(strategy.GetBestRound).ToList());

      Assert.Equal(0, scores.Sum(kv => kv.Value));
    }
  }
}
