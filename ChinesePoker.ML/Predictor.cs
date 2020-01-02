using System;
using System.Linq;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Model;

namespace ChinesePoker.ML
{
  public class Predictor
  {
    public void Go(string modelPath)
    {
      string line;
      do
      {
        var sets = Dealer.Deal().ToList();
        //var player = new CategorizationMlStrategy(modelPath);
        var player = new RegressionMlStrategy(modelPath);
        var playerSimple = new SimpleRoundStrategy();

        var mlRounds = player.GetBestRoundsWithScore(sets[0], 10).ToList();
        var simpleRounds = playerSimple.GetBestRounds(sets[0], 10).ToList();

        for (var i = 0; i < mlRounds.Count; i++)
        {
          Console.WriteLine($"{mlRounds[i].Value,-4:0} {mlRounds[i].Key}");
          Console.WriteLine($"     {simpleRounds[i]}");
          Console.WriteLine("======================");
        }

        line = Console.ReadLine();
        Console.Clear();
      } while (line != "q");
    }

    public void Simulation(string modelPath)
    {
      var mlStrategy = new RegressionMlStrategy(modelPath);
      var simpleStrategy = new SimpleRoundStrategy();
      var scoreKeeper = new TaiwaneseScoreCalculator(simpleStrategy.GameHandsManager.StrengthStrategy);

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

        for (var i = 0; i < 4; i++) Console.WriteLine($"{result[i].Key}\n{result[i].Value}");

        Console.WriteLine("---------------------------");
        rounds = new[] {round0, round2, round3, round4};
        result = scoreKeeper.GetScores(rounds).ToList();

        for (var i = 0; i < 4; i++) Console.WriteLine($"{result[i].Key}\n{result[i].Value}");

        line = Console.ReadLine();
        Console.Clear();
      } while (line != "q");
    }

    public void SimulationComparison(string modelPath)
    {
      var mlStrategy = new CategorizationMlStrategy(modelPath);
      var simpleStrategy = new SimpleRoundStrategy();
      var scoreKeeper = new TaiwaneseScoreCalculator(simpleStrategy.GameHandsManager.StrengthStrategy);

      int rA = 0, rB = 0;
      for (var k = 0; k < 100; k++)
      {
        var gameResultA = new int[4];
        var gameResultB = new int[4];

        var gameCount = 0;
        while (gameCount < 20)
        {
          var sets = Dealer.Deal().ToList();
          var round0 = mlStrategy.GetBestRound(sets[0]);
          var round1 = simpleStrategy.GetBestRound(sets[0]);
          var round2 = simpleStrategy.GetBestRound(sets[1]);
          var round3 = simpleStrategy.GetBestRound(sets[2]);
          var round4 = simpleStrategy.GetBestRound(sets[3]);

          var typeA = new[] {round0, round2, round3, round4};
          var typeB = new[] {round1, round2, round3, round4};

          var result = scoreKeeper.GetScores(typeA).ToList();
          for (var i = 0; i < 4; i++)
            gameResultA[i] += result[i].Value;

          result = scoreKeeper.GetScores(typeB).ToList();
          for (var i = 0; i < 4; i++)
            gameResultB[i] += result[i].Value;

          gameCount++;
        }

        rA += gameResultA[0];
        rB += gameResultB[0];

        Console.WriteLine($"{string.Join(" ", gameResultA)}");
        Console.WriteLine($"{string.Join(" ", gameResultB)}");
        Console.WriteLine(rA - rB);
        Console.WriteLine("-------------------");
      }

      Console.ReadLine();
    }

    public class CategorizationPredictionData
    {
      public int PredictedLabel { get; set; }
      //public float Probability { get; set; }
    }

    public class RegressionPredictionData
    {
      public float Score { get; set; }
    }
  }
}