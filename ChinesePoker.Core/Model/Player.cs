using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Interface;
using Combinatorics.Collections;

namespace ChinesePoker.Core.Model
{
  public class Player
  {
    public IList<Card> Cards { get; set; }

    public Player(IList<Card> cards)
    {
      Cards = cards;
    }


  }
}
