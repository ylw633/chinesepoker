using System.Collections.Generic;
using System.Linq;
using ChinesePoker.Core.Helper;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Component
{
  public class CategorizationMlStrategy : MlStrategyBase<CategorizationMlStrategy.PredictionData>
  {
    public CategorizationMlStrategy() : base(@"model-categorization.zip")
    {
    }

    public CategorizationMlStrategy(string trainedModelFilePath) : base(trainedModelFilePath)
    {
    }

    protected override Dictionary<Round, object> GetPrediction(IList<Card> cards)
    {
      return GameHandsManager.GetAllPossibleRounds(cards).ToDictionary(r => r, r => Oracle.Predict(new RoundData
      {
        FirstHandStrength = r.Hands[0].Strength,
        MiddleHandStrength = r.Hands.Count > 1 ? r.Hands[1].Strength : 0,
        LastHandStrength = r.Hands.Count > 1 ? r.Hands[2].Strength : 0
      }).PredictedLabel as object);
    }

    #region ML data class

    public class PredictionData
    {
      public int PredictedLabel { get; set; }
    }

    #endregion
  }
}