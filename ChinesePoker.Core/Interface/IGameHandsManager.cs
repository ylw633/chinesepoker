using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  public interface IGameHandsManager
  {
    IHandStrengthArbiter StrengthArbiter { get; }
    Hand DetermineHand(IEnumerable<Card> cards, Hand maxHand = null);
  }
}
