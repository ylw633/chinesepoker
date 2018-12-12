using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Model;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;

namespace ChinesePoker.ML
{
  public class Predictor
  {
    public void Go(string modelPath)
    {
      var sets = Dealer.Deal().ToList();
      var player = new SimpleRoundStrategy();

      var mlContext = new MLContext();
      ITransformer model;
      using (var sr = File.OpenRead(modelPath))
        model = mlContext.Model.Load(sr);

      var calc = model.MakePredictionFunction<Trainer.RoundData, Trainer.RoundStrengthPrediction>(mlContext);

      var predict = player.GetBestRounds(sets[0].ToList(), 7).Select(r => new
      {
        Round = r,
        Prediction = calc.Predict(new Trainer.RoundData
        {
          FirstHandStrength = r.Hands[0].Strength,
          MiddleHandStrength = r.Hands[0].Strength,
          LastHandStrength = r.Hands[0].Strength
        }).PredictedLabel
      }).ToList();


      foreach (var round in predict.OrderByDescending(p => p.Prediction))
      {
        Console.WriteLine($"{round.Round}\n{round.Prediction}");
      }

      Console.ReadLine();
    }

    public void Simulation(string modelPath)
    {
      var mlStrategy = new MachineLearningStrategy(modelPath);
      var simpleStrategy = new SimpleRoundStrategy();
      var scoreKeeper = new TaiwaneseScoreCalculator();

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
        var result = scoreKeeper.GetScore(rounds).ToList();

        for (int i = 0; i < 4; i++)
        {
          Console.WriteLine($"{result[i].Key}\n{result[i].Value}");
        }

        Console.WriteLine("---------------------------");
        rounds = new[] {round0, round2, round3, round4};
        result = scoreKeeper.GetScore(rounds).ToList();

        for (int i = 0; i < 4; i++)
        {
          Console.WriteLine($"{result[i].Key}\n{result[i].Value}");
        }

        line = Console.ReadLine();
        Console.Clear();
      } while (line != "q");
    }

    public void SimulationComparison(string modelPath)
    {
      var mlStrategy = new MachineLearningStrategy(modelPath);
      var simpleStrategy = new SimpleRoundStrategy();
      var scoreKeeper = new TaiwaneseScoreCalculator();

      int rA = 0, rB = 0;
      for (int k = 0; k < 100; k++)
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

          var result = scoreKeeper.GetScore(typeA).ToList();
          for (int i = 0; i < 4; i++)
            gameResultA[i] += result[i].Value;

          result = scoreKeeper.GetScore(typeB).ToList();
          for (int i = 0; i < 4; i++)
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
  }
}
