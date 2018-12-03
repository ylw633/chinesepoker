using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  interface IRoundStrategy
  {
    IEnumerable<Round> GetPossibleRounds(IList<Card> cards);
    Round GetBestRound(IList<Card> cards);
  }
}
