using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Helper;
using ChinesePoker.Core.Model;
using ChinesePoker.ML.Model;
using Microsoft.ML.Data;

namespace ChinesePoker.ML.Component
{
  public class CategorizationMlStrategy : MlStrategyBase<RoundData, CategorizationMlStrategy.PredictionData>
  {
    public CategorizationMlStrategy() : base(@"model-categorization.zip")
    {
    }

    public CategorizationMlStrategy(string trainedModelFilePath) : base(trainedModelFilePath)
    {
    }

    protected override Dictionary<Round, object> GetPrediction(IList<Card> cards)
    {
      VBuffer<int> keys = default;
      Oracle.OutputSchema.FirstOrDefault(c => c.Name == "PredictedLabel").GetKeyValues(ref keys);
      var labelsArray = keys.DenseValues().ToArray();

      return GameHandsManager.GetAllPossibleRounds(cards).ToDictionary(r => r, r =>
      {
        var score = Oracle.Predict(new RoundData(r)).Score.ToList();
        return labelsArray[score.IndexOf(score.Max())] as object;
      });
    }

    #region ML data class

    public class PredictionData
    {
      public float[] Score { get; set; }
    }
    #endregion
  }
}