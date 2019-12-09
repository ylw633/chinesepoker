using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Helper;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Data;

namespace ChinesePoker.Core.Component
{
  public abstract class MlStrategyBase<T> : IRoundStrategy where T : class, new()
  {
    protected PredictionFunction<RoundData, T> Oracle { get; set; }
    public IGameHandsManager GameHandsManager { get; set; } = new PokerHandBuilderManager();
    protected virtual Func<IEnumerable<KeyValuePair<Round, object>>, IOrderedEnumerable<KeyValuePair<Round, object>>> Ordering { get; } = enu => enu.OrderByDescending(r => r.Value).ThenByDescending(r => r.Key.Strength);

    protected MlStrategyBase(string trainedModelFilePath)
    {
      var mlContext = new MLContext();
      ITransformer model;
      using (var sr = File.OpenRead(trainedModelFilePath))
        model = mlContext.Model.Load(sr);
      Oracle = model.MakePredictionFunction<RoundData, T>(mlContext);
    }

    protected abstract Dictionary<Round, object> GetPrediction(IList<Card> cards);

    public IEnumerable<Round> GetPossibleRounds(IList<Card> cards)
    {
      return GetPrediction(cards).GroupBy(r => string.Join("_", r.Key.Hands.Select(h => h.Name))).Select(typeCombo => Ordering(typeCombo).First().Key);
    }

    public IEnumerable<Round> GetBestRounds(IList<Card> cards, int take = 1)
    {
      var rounds = GetPrediction(cards).GroupBy(r => string.Join("_", r.Key.Hands.Select(h => h.Name))).Select(typeCombo => Ordering(typeCombo).First());
      return Ordering(rounds).Take(take).Select(r => r.Key);
    }

    public IEnumerable<KeyValuePair<Round, object>> GetBestRoundsWithScore(IList<Card> cards, int take = 1)
    {
      var rounds = GetPrediction(cards).GroupBy(r => string.Join("_", r.Key.Hands.Select(h => h.Name))).Select(typeCombo => Ordering(typeCombo).First());
      return Ordering(rounds).Take(take);
    }

    public Round GetBestRound(IList<Card> cards)
    {
      return GetBestRounds(cards).First();
    }
  }


  public class RegressionMlStrategy : MlStrategyBase<RegressionMlStrategy.PredictionData>
  {
    #region ML data class

    public class PredictionData
    {
      public float Score { get; set; }
    }
    
    #endregion

    public RegressionMlStrategy() : base(@"model-regression.zip")
    {
    }

    public RegressionMlStrategy(string trainedModelFilePath) : base(trainedModelFilePath)
    {
    }

    protected override Dictionary<Round, object> GetPrediction(IList<Card> cards)
    {
      return GameHandsManager.GetAllPossibleRounds(cards).ToDictionary(r => r, r => Oracle.Predict(new RoundData
      {
        FirstHandStrength = r.Hands[0].Strength,
        MiddleHandStrength = r.Hands.Count > 1 ? r.Hands[1].Strength : 0,
        LastHandStrength = r.Hands.Count > 1 ? r.Hands[2].Strength : 0
      }).Score as object);
    }
  }
}
