using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  public interface IScoreCalculator
  {
    void GetScore(Round roundA, Round roundB, out int scoreA, out int scoreB);
  }
}
