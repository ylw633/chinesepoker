using System.Collections.Generic;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using ChinesePoker.ML.Component;
using System.Linq;
using Console = System.Console;

namespace ChinesePoker.Console
{
  public class Predictor
  {
    public void Go(IRoundStrategy player)
    {
      string line;
      do
      {
        var sets = Dealer.Deal().ToList();
        //var set = Dealer.GetCards("SA HT CQ D2 H3 H4 S5 S6 H5 H7 HJ HQ HK");
        //sets[0] = set;
        var playerSimple = new SimpleRoundStrategy();

        var mlRounds = player.GetBestRoundsWithScore(sets[0], 10).ToList();
        var simpleRounds = playerSimple.GetBestRounds(sets[0], 10).ToList();

        for (var i = 0; i < mlRounds.Count; i++)
        {
          System.Console.WriteLine($"{mlRounds[i].Value,-4:0} {mlRounds[i].Key}");
          System.Console.WriteLine($"     {simpleRounds[i]}");
          System.Console.WriteLine("======================");
        }

        line = System.Console.ReadLine();
        System.Console.Clear();
      } while (line != "q");
    }

    public void Simulation(string modelPath)
    {
      var mlStrategy = new RegressionMlStrategy(modelPath);
      var simpleStrategy = new SimpleRoundStrategy();
      var scoreKeeper = new TaiwaneseScoreCalculator(simpleStrategy.GameHandsManager.StrengthArbiter);

      string line;
      do
      {
        var sets = Dealer.Deal().ToList();

        var round1 = mlStrategy.GetBestRound(sets[0]);
        var round0 = simpleStrategy.GetBestRound(sets[0]);
        var round2 = simpleStrategy.GetBestRound(sets[1]);
        var round3 = simpleStrategy.GetBestRound(sets[2]);
        var round4 = simpleStrategy.GetBestRound(sets[3]);

        var rounds = new[] {round1, round2, round3, round4};
        var result = scoreKeeper.GetScores(rounds).ToList();

        for (var i = 0; i < 4; i++) System.Console.WriteLine($"{result[i].Key}\n{result[i].Value}");

        System.Console.WriteLine("---------------------------");
        rounds = new[] {round0, round2, round3, round4};
        result = scoreKeeper.GetScores(rounds).ToList();

        for (var i = 0; i < 4; i++) System.Console.WriteLine($"{result[i].Key}\n{result[i].Value}");

        line = System.Console.ReadLine();
        System.Console.Clear();
      } while (line != "q");
    }

    public void SimulationComparison(IEnumerable<IRoundStrategy> player)
    {
      var scoreCalculator = new TaiwaneseScoreCalculator(new PokerHandBuilderManager().StrengthArbiter);

      var scoreKeeper = new [] { new SimpleRoundStrategy() }.Concat(player).Select(s => new ScoreKeeper {Strategy = s}).ToList();

      int rA = 0, rB = 0;
      for (var k = 0; k < 100; k++)
      {
        foreach (var s in scoreKeeper)
          s.TempScore = new int[4];

        var gameCount = 0;
        while (gameCount < 20)
        {
          var sets = Dealer.Deal().ToList();
          var round2 = scoreKeeper[0].Strategy.GetBestRound(sets[1]);
          var round3 = scoreKeeper[0].Strategy.GetBestRound(sets[2]);
          var round4 = scoreKeeper[0].Strategy.GetBestRound(sets[3]);

          foreach (var role in scoreKeeper)
          {
            var round = role.Strategy.GetBestRound(sets[0]);
            var result = scoreCalculator.GetScores(new[] {round, round2, round3, round4}).Select(r => r.Value).ToList();
            for (var i = 0; i < 4; i++)
              role.TempScore[i] += result[i];
          }

          //var typeA = new[] {round0, round2, round3, round4};
          //var typeB = new[] {round1, round2, round3, round4};

          //var result = scoreCalculator.GetScores(typeA).ToList();
          //for (var i = 0; i < 4; i++)
          //  gameResultA[i] += result[i].Value;

          //result = scoreCalculator.GetScores(typeB).ToList();
          //for (var i = 0; i < 4; i++)
          //  gameResultB[i] += result[i].Value;

          gameCount++;
        }

        foreach (var role in scoreKeeper)
        {
          role.TotalScore += role.TempScore[0];
        }

        foreach (var role in scoreKeeper)
        {
          System.Console.WriteLine($"{role.Strategy.GetType().Name.Substring(0, 5)} {role.TotalScore - scoreKeeper[0].TotalScore,4} {role.TotalScore,5} {role.TempScore[0],4} {role.TempScore[1],4} {role.TempScore[2],4} {role.TempScore[3],4}");
        }

        System.Console.WriteLine("-------------------");
      }

      System.Console.ReadLine();
    }

    public class ScoreKeeper
    {
      public IRoundStrategy Strategy { get; set; }
      public int TotalScore { get; set; }
      public int[] TempScore { get; set; }
    }
  }
}