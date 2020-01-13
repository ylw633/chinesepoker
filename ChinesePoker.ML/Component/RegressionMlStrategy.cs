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
  public class RegressionMlStrategy : MlStrategyBase<RoundData<float>, RegressionMlStrategy.PredictionData>
  {
    public RegressionMlStrategy() : base(@"model-regression.zip")
    {
    }

    public RegressionMlStrategy(string trainedModelFilePath) : base(trainedModelFilePath)
    {
    }

    protected override Dictionary<Round, int> GetPrediction(IList<Card> cards)
    {
      var rounds = new SimpleRoundStrategy().GetBestRounds(cards, int.MaxValue).ToList();
      var result = new Dictionary<Round, int>();
      for (var i = 0; i < rounds.Count; i++)
      {
        var predict = Oracle.Predict(new RoundData<float>(rounds[i], i));
        result.Add(rounds[i], (int)Math.Round(predict.Score));
      }

      return result;
    }

    #region ML data class

    public class PredictionData
    {
      [ColumnName("Score")]
      public float Score { get; set; }
    }
    
    #endregion
  }
}