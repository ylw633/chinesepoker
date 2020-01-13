using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Microsoft.ML;

namespace ChinesePoker.ML.Component
{
  public abstract class MlStrategyBase<TModel, TPrediction> : IRoundStrategy
    where TModel : class where TPrediction : class, new()
  {
    protected MlStrategyBase(string trainedModelFilePath)
    {
      var mlContext = new MLContext();
      ITransformer trainedModel;
      using (var sr = File.OpenRead(trainedModelFilePath))
      {
        trainedModel = mlContext.Model.Load(sr, out _);
      }

      Oracle = mlContext.Model.CreatePredictionEngine<TModel, TPrediction>(trainedModel);
    }

    protected PredictionEngine<TModel, TPrediction> Oracle { get; set; }
    public IGameHandsManager GameHandsManager { get; set; } = new PokerHandBuilderManager();

    protected virtual Func<IEnumerable<KeyValuePair<Round, int>>, IOrderedEnumerable<KeyValuePair<Round, int>>>
      Ordering { get; } = enu => enu.OrderByDescending(r => r.Value).ThenByDescending(r => r.Key.Strength);

    public IEnumerable<Round> GetPossibleRounds(IList<Card> cards)
    {
      return GetPrediction(cards).GroupBy(r => string.Join("_", r.Key.Hands.Select(h => h.Name)))
        .Select(typeCombo => Ordering(typeCombo).First().Key);
    }

    public IEnumerable<Round> GetBestRounds(IList<Card> cards, int take = 1)
    {
      var rounds = GetPrediction(cards).GroupBy(r => string.Join("_", r.Key.Hands.Select(h => h.Name)))
        .Select(typeCombo => Ordering(typeCombo).First());
      return Ordering(rounds).Take(take).Select(r => r.Key);
    }

    public IEnumerable<KeyValuePair<Round, int>> GetBestRoundsWithScore(IList<Card> cards, int take = 1)
    {
      var rounds = GetPrediction(cards).GroupBy(r => string.Join("_", r.Key.Hands.Select(h => h.Name)))
        .Select(typeCombo => Ordering(typeCombo).First());
      return Ordering(rounds).Take(take);
    }

    protected abstract Dictionary<Round, int> GetPrediction(IList<Card> cards);

    public Round GetBestRound(IList<Card> cards)
    {
      return GetBestRounds(cards).First();
    }
  }
}