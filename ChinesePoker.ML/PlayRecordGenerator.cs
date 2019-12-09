using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Component;
using ChinesePoker.Core.Model;
using Combinatorics.Collections;

namespace ChinesePoker.ML
{
  public class PlayRecordGenerator
  {
    public int BestRoundsToTake { get; set; } = 7;
    private StreamWriter Writer { get; set; }
    public void Go(string outFileName, int recordNeeded = 1_000_000)
    {
      var count = 0;
      var strategy = new SimpleRoundStrategy();
      var scoreKeeper = new TaiwaneseScoreCalculator();
      var timer = new Stopwatch();
      timer.Start();
      Writer = new StreamWriter(outFileName, true);
      while (count < recordNeeded)
      {
        //using (var sw = new StreamWriter(outFileName, true))
        {
          var players = Dealer.Deal().ToList();
          foreach (var p1Rounds in strategy.GetBestRounds(players[0].ToList(), BestRoundsToTake))
          foreach (var p2Rounds in strategy.GetBestRounds(players[1].ToList(), BestRoundsToTake))
          foreach (var p3Rounds in strategy.GetBestRounds(players[2].ToList(), BestRoundsToTake))
          foreach (var p4Rounds in strategy.GetBestRounds(players[3].ToList(), BestRoundsToTake))
          {
            var result = scoreKeeper.GetScores(new[] {p1Rounds, p2Rounds, p3Rounds, p4Rounds});
            foreach (var r in result)
              OutputResult(Writer, r.Key, r.Value);

            count += 4;
            if (count % 10_000 == 0)
            {
              Console.WriteLine($"count {count}, time: {timer.Elapsed}");
            }
          }
        }
      }
      timer.Stop();
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
      Writer.Flush();
      Writer.Dispose();
    }
  }
}
