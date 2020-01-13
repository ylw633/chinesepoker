using System;
using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Helper;
using ChinesePoker.Core.Model;
using ChinesePoker.ML.Model;
using Microsoft.ML.Data;

namespace ChinesePoker.ML.Component
{
  public class CategorizationMlStrategy : MlStrategyBase<RoundData<int>, CategorizationMlStrategy.PredictionData>
  {
    public CategorizationMlStrategy() : base(@"model-categorization.zip")
    {
    }

    public CategorizationMlStrategy(string trainedModelFilePath) : base(trainedModelFilePath)
    {
    }

    protected override Dictionary<Round, int> GetPrediction(IList<Card> cards)
    {
      VBuffer<int> keys = default;
      Oracle.OutputSchema.FirstOrDefault(c => c.Name == "PredictedLabel").GetKeyValues(ref keys);
      var labelsArray = keys.DenseValues().ToArray();

      var rounds = new SimpleRoundStrategy().GetBestRounds(cards, int.MaxValue).ToList();
      var result = new Dictionary<Round, int>();
      for (var i = 0; i < rounds.Count; i++)
      {
        var predict = Oracle.Predict(new RoundData<int>(rounds[i], i));
        result.Add(rounds[i], labelsArray[predict.Score.ToList().IndexOf(predict.Score.Max())]);
      }

      return result;
    }

    #region ML data class

    public class PredictionData
    {
      public int PredictedLabel { get; set; }
      public float[] Score { get; set; }
    }
    #endregion
  }
}