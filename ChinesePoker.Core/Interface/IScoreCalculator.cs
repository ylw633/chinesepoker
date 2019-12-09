﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Interface
{
  public interface IScoreCalculator
  {
    int GetScore(Round roundA, Round roundB, out int scoreA, out int scoreB);
    Dictionary<Round, int> GetScores(IList<Round> rounds);
  }
}
