using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  public interface IHandBuilder
  {
    string HandName { get; }
    IStrengthStrategy StrengthStrategy { get; set; }
    bool TestIsHand(IList<Card> cards);

    IEnumerable<string> GetAllPossibleComboSorted();
    //IList<Card> SortCards(IList<Card> cards);
    //int GetStrength(IList<Card> cards);
    //int CompareHands(Hand srcHand, Hand targetHand);
    //Hand GetHand(IList<Card> cards, int handStrengthOffset = 0);
  }
}
