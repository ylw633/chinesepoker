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
        }).Score
      }).ToList();


      foreach (var round in predict.OrderByDescending(p => p.Prediction))
      {
        Console.WriteLine($"{round.Round}\n{round.Prediction}");
      }

      Console.ReadLine();
    }
  }
}
