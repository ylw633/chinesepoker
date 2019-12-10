using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Component.HandBuilders;
using ChinesePoker.Core.Model;
using Combinatorics.Collections;
using Common;

namespace ChinesePoker.ML
{
  public class PlayRecordGenerator
  {
    public int BestRoundsToTake { get; set; } = 4;
    private StreamWriter Writer { get; set; }
    public void Go(string outFileName, int recordNeeded = 1_000_000)
    {
      var count = 0;
      var strategy = new SimpleRoundStrategy();
      var scoreKeeper = new TaiwaneseScoreCalculator(strategy.GameHandsManager.StrengthStrategy);
      var timer = new Stopwatch();
      timer.Start();
      Writer = new StreamWriter(outFileName, true);

      var players = Dealer.Deal().ToList();
      var playerRounds = new SynchronizedCollection<IList<Round>>();

      while (count < recordNeeded)
      {
        Parallel.ForEach(players, p =>
        {
          playerRounds.Add(strategy.GetBestRounds(p.ToList(), BestRoundsToTake).ToList());
        });

        Parallel.ForEach(playerRounds[0], p1Round =>
        Parallel.ForEach(playerRounds[1], p2Round =>
        Parallel.ForEach(playerRounds[2], p3Round =>
        Parallel.ForEach(playerRounds[3], p4Round =>
        {
          var result = scoreKeeper.GetScores(new[] { p1Round, p2Round, p3Round, p4Round });
          lock (this)
          {
            foreach (var r in result)
              OutputResult(Writer, r.Key, r.Value);
            count += 4;
            if (count % 10_000 == 0)
            {
              Console.WriteLine($"count {count}, time: {timer.Elapsed}");
            }

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
      writer.WriteLine(round.Hands.Count == 3
        ? $"{round.Hands[0].Strength} {round.Hands[1].Strength} {round.Hands[2].Strength} {score}"
        : $"{round.Hands[0].Strength} 0 0 {score}");
    }

    ~PlayRecordGenerator()
    {
      Writer.Dispose();
    }
  }
}
