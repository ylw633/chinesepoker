using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Helper;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;

namespace ChinesePoker.Core.Component
{
  public class MachineLearningStrategy : IRoundStrategy
  {
    #region ML data class

    public class RoundData
    {
      public int FirstHandStrength { get; set; }
      public int MiddleHandStrength { get; set; }
      public int LastHandStrength { get; set; }
      public int Score { get; set; }
    }

    public class RoundStrengthPrediction
    {
      public int PredictedLabel { get; set; }
    }
    
    #endregion

    private PredictionFunction<RoundData, RoundStrengthPrediction> Oracle { get; set; }
    public IGameHandsManager GameHandsManager { get; set; } = new PokerHandBuilderManager();

    private Func<IEnumerable<KeyValuePair<Round, int>>, IOrderedEnumerable<KeyValuePair<Round, int>>> Ordering = enu => enu.OrderByDescending(r => r.Value).ThenByDescending(r => r.Key.TotalStrength);

    public MachineLearningStrategy() : this(@"model-categorization.zip")
    {
    }

    public MachineLearningStrategy(string trainedModelFilePath)
    {
      var mlContext = new MLContext();
      ITransformer model;
      using (var sr = File.OpenRead(trainedModelFilePath))
        model = mlContext.Model.Load(sr);
      Oracle = model.MakePredictionFunction<RoundData, RoundStrengthPrediction>(mlContext);
    }

    private Dictionary<Round, int> GetPrediction(IList<Card> cards)
    {
      return GameHandsManager.GetAllPossibleRounds(cards).ToDictionary(r => r, r => Oracle.Predict(new RoundData
      {
        FirstHandStrength = r.Hands[0].Strength,
        MiddleHandStrength = r.Hands[1].Strength,
        LastHandStrength = r.Hands[2].Strength
      }).PredictedLabel);
    }
    public IEnumerable<Round> GetPossibleRounds(IList<Card> cards)
    {
      return GetPrediction(cards).GroupBy(r => string.Join("_", r.Key.Hands.Select(h => h.Name))).Select(typeCombo => Ordering(typeCombo).First().Key);
    }

    public IEnumerable<Round> GetBestRounds(IList<Card> cards, int take = 1)
    {
      var rounds = GetPrediction(cards).GroupBy(r => string.Join("_", r.Key.Hands.Select(h => h.Name))).Select(typeCombo => Ordering(typeCombo).First());
      return Ordering(rounds).Take(take).Select(r => r.Key);
    }

    public Round GetBestRound(IList<Card> cards)
    {
      return GetBestRounds(cards).First();
    }
  }
}
