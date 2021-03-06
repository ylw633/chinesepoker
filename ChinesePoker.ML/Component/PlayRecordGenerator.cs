﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Interface;
using ChinesePoker.Core.Model;
using Common;

namespace ChinesePoker.ML.Component
{
  public class PlayRecordGenerator
  {
    public int BestRoundsToTake { get; set; } = 4;
    private StreamWriter Writer { get; set; }

    public void Go(string outFileName, int recordNeeded = 1_000_000)
    {
      var count = 0;
      var strategy = new SimpleRoundStrategy();
      var scoreKeeper = new TaiwaneseScoreCalculator(strategy.GameHandsManager.StrengthArbiter);
      var timer = new Stopwatch();
      timer.Start();
      Writer = new StreamWriter(outFileName, true);

      var gamesNeeded = Math.Max(1, (int)Math.Round(recordNeeded / 4.0 / Math.Pow(BestRoundsToTake, 4)) + 1);
      Parallel.For(0, gamesNeeded, _ =>
      {
        var players = Dealer.Deal().ToList();
        var playerRounds = players.Select(p => strategy.GetBestRounds(p, BestRoundsToTake).ToList()).ToList();
        for (var p1 = 0; p1 < playerRounds[0].Count; p1++)
        for (var p2 = 0; p2 < playerRounds[1].Count; p2++)
        for (var p3 = 0; p3 < playerRounds[2].Count; p3++)
        for (var p4 = 0; p4 < playerRounds[3].Count; p4++)
        {
          //var result = scoreKeeper.GetScores(new[] {playerRounds[0][p1], playerRounds[1][p2], playerRounds[2][p3], playerRounds[3][p4]}.ToList()).ToList();
          var result = scoreKeeper.GetScoresWithRoundWeight(new[] { playerRounds[0][p1], playerRounds[1][p2], playerRounds[2][p3], playerRounds[3][p4] }.ToList()).ToList();
          lock (this)
          {
            for (var i = 0; i < result.Count; i++)
              OutputResult(Writer, result, i, new int[] { p1, p2, p3, p4 });
            count += 4;
            if (count % 10_000 == 0) Console.WriteLine($"count {count}, time: {timer.Elapsed}");
          }
        }
      });

      //var players = Dealer.Deal().ToList();
      //var playerRounds = new SynchronizedCollection<IList<Round>>();

      //while (count < recordNeeded)
      //{
      //  Parallel.ForEach(players, p => { playerRounds.Add(strategy.GetBestRounds(p.ToList(), BestRoundsToTake).ToList()); });

      //  Parallel.For(0, playerRounds[0].Count, p1 =>
      //  Parallel.For(0, playerRounds[1].Count, p2 =>
      //  Parallel.For(0, playerRounds[2].Count, p3 =>
      //  Parallel.For(0, playerRounds[3].Count, p4 =>
      //  {
      //    var result = scoreKeeper.GetScores(new[] {playerRounds[0][p1], playerRounds[1][p2], playerRounds[2][p3], playerRounds[3][p4]}.ToList()).ToList();
      //    lock (this)
      //    {
      //      for (var i = 0; i < result.Count; i++)
      //        OutputResult(Writer, result, i, new int[] {p1,p2,p3,p4});
      //      count += 4;
      //      if (count % 10_000 == 0) Console.WriteLine($"count {count}, time: {timer.Elapsed}");
      //    }
      //  }))));
      //}

      timer.Stop();
      //ConsoleHelper.ConsolePressAnyKey();
      Writer.Flush();
      Writer.Dispose();
    }

    public void OutputResult(StreamWriter writer, IList<KeyValuePair<Round, PlayerScore>> result, int index, IEnumerable<int> roundIndex)
    {
      var score = result[index].Value;
      var round = result[index].Key;

      writer.Write($"{score.TotalScore},");
      if (round.Hands.Count < 3)
      {
        writer.Write($"{round.Hands[0].Strength},0,0,{round.Hands[0].Name},,,{string.Join(",", round.Hands[0].Cards)}");
      }
      else
      {
        writer.Write($"{round.Hands[0].Strength},{round.Hands[1].Strength},{round.Hands[2].Strength},{round.Hands[0].Name},{round.Hands[1].Name},{round.Hands[2].Name},{string.Join(",", round.Hands[0].Cards)},{string.Join(",", round.Hands[1].Cards)},{string.Join(",", round.Hands[2].Cards)}");
      }

      Writer.WriteLine($",{index},{string.Join(",", roundIndex)},{string.Join(",",score.RoundWeight)}");
    }

    ~PlayRecordGenerator()
    {
      Writer.Dispose();
    }
  }
}