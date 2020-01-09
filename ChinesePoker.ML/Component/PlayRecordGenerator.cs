using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChinesePoker.Core.Component;
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

      var players = Dealer.Deal().ToList();
      var playerRounds = new SynchronizedCollection<IList<Round>>();

      while (count < recordNeeded)
      {
        Parallel.ForEach(players,
          p => { playerRounds.Add(strategy.GetBestRounds(p.ToList(), BestRoundsToTake).ToList()); });

        Parallel.ForEach(playerRounds[0], p1Round =>
        Parallel.ForEach(playerRounds[1], p2Round =>
        Parallel.ForEach(playerRounds[2], p3Round =>
        Parallel.ForEach(playerRounds[3], p4Round =>
        {
          var result = scoreKeeper.GetScores(new[] {p1Round, p2Round, p3Round, p4Round});
          lock (this)
          {
            foreach (var r in result)
              OutputResult(Writer, r.Key, r.Value);
            count += 4;
            if (count % 10_000 == 0) Console.WriteLine($"count {count}, time: {timer.Elapsed}");
          }
        }))));
      }

      timer.Stop();
      ConsoleHelper.ConsolePressAnyKey();
      Writer.Flush();
      Writer.Dispose();
    }

    public void OutputResult(StreamWriter writer, Round round, int score)
    {
      writer.Write($"{score},");
      if (round.Hands.Count < 3)
      {
        writer.WriteLine($"{round.Hands[0].Strength},0,0,{round.Hands[0].Name},,,{string.Join(",", round.Hands[0].Cards)}");
      }
      else
      {
        writer.WriteLine($"{round.Hands[0].Strength},{round.Hands[1].Strength},{round.Hands[2].Strength},{round.Hands[0].Name},{round.Hands[1].Name},{round.Hands[2].Name},{string.Join(",", round.Hands[0].Cards)},{string.Join(",", round.Hands[1].Cards)},{string.Join(",", round.Hands[2].Cards)}");
      }
    }

    ~PlayRecordGenerator()
    {
      Writer.Dispose();
    }
  }
}