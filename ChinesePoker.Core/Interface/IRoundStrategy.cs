using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  public interface IRoundStrategy
  {
    IEnumerable<Round> GetPossibleRounds(IList<Card> cards);
    IEnumerable<Round> GetBestRounds(IList<Card> cards, int take = 1);
    Round GetBestRound(IList<Card> cards);
    IEnumerable<KeyValuePair<Round, object>> GetBestRoundsWithScore(IList<Card> cards, int take = 1);
  }
}
