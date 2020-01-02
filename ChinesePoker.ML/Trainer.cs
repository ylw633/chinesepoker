using System;
using System.IO;
using ChinesePoker.Core.Model;
using Common;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;

namespace ChinesePoker.ML
{
  public class Trainer
  {
    //public void CategorizationTraining(string dataFileName, string modelPath)
    //{
    //  var mlContext = new MLContext();
    //  var trainingDataView = mlContext.Data.LoadFromTextFile<RoundData>(dataFileName, ' ');

    //  var dataPipeline = mlContext.Transforms.Conversion.MapValueToKey(nameof(RoundData.Score), "Label").Append(CategoricalCatalog.OneHotEncoding(mlContext.Transforms.Categorical, nameof(RoundData.FirstHandStrength)))
    //    .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.MiddleHandStrength)))
    //    .Append(mlContext.Transforms.Categorical.OneHotEncoding(nameof(RoundData.LastHandStrength)))
    //    .Append(mlContext.Transforms.Concatenate("Features",
    //      nameof(RoundData.FirstHandStrength),
    //      nameof(RoundData.MiddleHandStrength),
    //      nameof(RoundData.LastHandStrength)))
    //    .AppendCacheCheckpoint(mlContext);
    //  var trainer = mlContext.MulticlassClassification.Trainers.NaiveBayes();

    //  //var averagedPerceptronBinaryTrainer = mlContext.BinaryClassification.Trainers.AveragedPerceptron(numIterations: 10);
    //  //var trainer = mlContext.MulticlassClassification.Trainers.OneVersusAll(averagedPerceptronBinaryTrainer);

    //  var trainerPipeline = dataPipeline.Append(trainer)
    //    .Append(ConversionsExtensionsCatalog.MapKeyToValue(mlContext.Transforms.Conversion, "PredictedLabel"));


    //  var preview = dataView.Preview();
    //  var transPreview = DebuggerExtensions.Preview(trainerPipeline, dataView);

    //  //var crossValidationResults = mlContext.MulticlassClassification.CrossValidate(dataView, trainerPipeline);
    //  //ConsoleHelper.PrintMulticlassClassificationFoldsAverageMetrics(trainer.ToString(), crossValidationResults);

    //  var model = trainerPipeline.Fit(dataView);

    //  using (var sw = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
    //  {
    //    mlContext.Model.Save(model, sw);
    //  }

    //  ConsoleHelper.ConsolePressAnyKey();
    //}

    private void Training<TM, TT>(string dataFileName, string modelPath, Func<MLContext, TT> appendTrainer) where TT : IEstimator<ITransformer>
    {
      var mlContext = new MLContext();
      var trainingDataView = mlContext.Data.LoadFromTextFile<TM>(dataFileName, ' ');

      var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(RoundData.Score))
        .Append(mlContext.Transforms.Concatenate("Features", nameof(RoundData.FirstHandStrength), nameof(RoundData.MiddleHandStrength), nameof(RoundData.LastHandStrength)))
        .Append(mlContext.Transforms.Conversion.MapKeyToValue(nameof(RoundData.Score), "Label"));

      
      //ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 5);
      //ConsoleHelper.PeekVectorColumnDataInConsole(mlContext, "Features", trainingDataView, dataProcessPipeline, 5);

      //var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features");
      var trainingPipeline = dataProcessPipeline.Append(appendTrainer(mlContext));

      var trainedModel = trainingPipeline.Fit(trainingDataView);
      //var trainedModel = dataProcessPipeline.Fit(trainingDataView);

      //var testDataView = mlContext.Data.LoadFromTextFile<RoundData>(@"D:\ws\temp\cpRecords.txt", ' ', false);

      //IDataView predictions = trainedModel.Transform(testDataView);
      //var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");
      //Common.ConsoleHelper.PrintRegressionMetrics(trainer.ToString(), metrics);


      using (var sw = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
      {
        mlContext.Model.Save(trainedModel, trainingDataView.Schema, sw);
      }

      //ConsoleHelper.ConsolePressAnyKey();
    }

    public void RegressionTraining(string dataFileName, string modelPath)
    {
      Training<RoundData, IEstimator<ITransformer>>(dataFileName, GetRegressionModelPath(modelPath), mlContext => mlContext.Regression.Trainers.Sdca());
    }

    public void RankingTraining(string dataFileName, string modelPath)
    {
      Training<RoundData, IEstimator<ITransformer>>(dataFileName, GetrankingModelPath(modelPath), mlContext => mlContext.Ranking.Trainers.FastTree());
    }

    public void CategorizationTraining(string dataFileName, string modelPath)
    {
      Training<CatRoundData, IEstimator<ITransformer>>(dataFileName, GetCategorizationModelPath(modelPath), mlContext => mlContext.MulticlassClassification.Trainers.NaiveBayes());
    }

    public static string GetRegressionModelPath(string modelPath)
    {
      return Path.Combine(modelPath, "cpmodel-regression.zip");
    }

    public static string GetrankingModelPath(string modelPath)
    {
      return Path.Combine(modelPath, "cpmodel-ranking.zip");
    }

    public static string GetCategorizationModelPath(string modelPath)
    {
      return Path.Combine(modelPath, "cpmodel-categorization.zip");
    }

    public class RoundData
    {
      public RoundData(int result)
      {
        Score = result;
      }

      [LoadColumn(0)]
      public float FirstHandStrength { get; set; }
      [LoadColumn(1)]
      public float MiddleHandStrength { get; set; }
      [LoadColumn(2)]
      public float LastHandStrength { get; set; }
      [LoadColumn(3)]
      public float Score { get; set; }
    }

    public class CatRoundData
    {
      public CatRoundData(int result)
      {
        Score = result;
      }

      [LoadColumn(0)]
      public float FirstHandStrength { get; set; }
      [LoadColumn(1)]
      public float MiddleHandStrength { get; set; }
      [LoadColumn(2)]
      public float LastHandStrength { get; set; }
      [LoadColumn(3)]
      public int Score { get; set; }
    }
  }
}