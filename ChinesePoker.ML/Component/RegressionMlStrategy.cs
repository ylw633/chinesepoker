using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Helper;
using ChinesePoker.Core.Model;
using ChinesePoker.ML.Model;
using Microsoft.ML.Data;

namespace ChinesePoker.ML.Component
{
  public class RegressionMlStrategy : MlStrategyBase<RoundData, RegressionMlStrategy.PredictionData>
  {
    public RegressionMlStrategy() : base(@"model-regression.zip")
    {
    }

    public RegressionMlStrategy(string trainedModelFilePath) : base(trainedModelFilePath)
    {
    }

    protected override Dictionary<Round, object> GetPrediction(IList<Card> cards)
    {
      return GameHandsManager.GetAllPossibleRounds(cards).ToDictionary(r => r, r => Oracle.Predict(new RoundData(r)).Score as object);
    }

    #region ML data class

    public class PredictionData
    {
      [ColumnName("Score")]
      public float[] Score { get; set; }
    }
    
    #endregion
  }
}