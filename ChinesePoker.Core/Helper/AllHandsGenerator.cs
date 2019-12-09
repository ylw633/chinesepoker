using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinesePoker.Core.Model;

namespace ChinesePoker.Core.Helper
{
	public class AllHandsGenerator
  {
    public void Generate()
    {
      var genFuncList = new List<(Func<IEnumerable<string>> getFunc, IEnumerable<Func<string, string>> compDelegate)>
      {
        (AllHighCards5, new Func<string, string>[] { f => f}),
        (AllHighCards3, new Func<string, string>[] { f => f}),
        (AllOnePair5, new Func<string, string>[] { s => s[0].ToString(), s => s.Substring(1) }),
        (AllOnePair3, new Func<string, string>[] { s => s[0].ToString(), s => s[2].ToString() }),
        (AllTwoPairs, new Func<string, string>[] { s => s[0].ToString(), s => s[2].ToString(), s => s[4].ToString() }),
        (AllThreeOfKind5, new Func<string, string>[] { s => s[0].ToString(), s => s.Substring(3) }),
        (AllThreeOfKind3, new Func<string, string>[] { s => s }),
        (AllFullHouse, new Func<string, string>[] { s => s[0].ToString(), s => s[3].ToString() }),
        (AllFourOfAKind, new Func<string, string>[] { s => s[0].ToString(), s => s[4].ToString() }),
      };

      var genFunc = genFuncList[8];
      Print(genFunc.getFunc, genFunc.compDelegate);
		}

    public void Print(Func<IEnumerable<string>> getFunc, IEnumerable<Func<string, string>> compDelegate)
    {
      var cardComparer = new CardComparer(compDelegate);
      int i = 0;
			foreach (var a in getFunc().OrderBy(s => s, cardComparer))
      {
        Console.Write($"\"{new string(a.OrderBy(Card.RankToOrdinal).ToArray())}\",");
        i = (i + 1) % 20;
        if (i == 0) Console.WriteLine();
      };
    }

		protected IEnumerable<string> AllHighCards5()
		{
			for (int i = 1; i < 10; i++)
				for (int j = i + 1; j < 11; j++)
					for (int k = j + 1; k < 12; k++)
						for (int l = k + 1; l < 13; l++)
							for (int m = l + 1; m < 14; m++)
							{
								if (j == i + 1 && k == j + 1 && l == k + 1 && m == l + 1) continue; // filter pair
								if (i == 1 && j == 10 && k == 11 && l == 12 && m == 13) continue; // filter straight 

								yield return ($"{Card.OrdinalToRank(i)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(k)}{Card.OrdinalToRank(l)}{Card.OrdinalToRank(m)}");
							}
		}

		protected IEnumerable<string> AllHighCards3()
		{
			for (int k = 1; k < 12; k++)
				for (int l = k + 1; l < 13; l++)
					for (int m = l + 1; m < 14; m++)
					{
						yield return ($"{Card.OrdinalToRank(k)}{Card.OrdinalToRank(l)}{Card.OrdinalToRank(m)}");
					}
		}

		protected IEnumerable<string> AllOnePair5()
		{
			for (int j = 1; j < 14; j++)
				for (int k = 1; k < 12; k++)
					for (int l = k + 1; l < 13; l++)
						for (int m = l + 1; m < 14; m++)
						{
							if (j == k || l == j || m == j) continue;
							yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}" + ($"{Card.OrdinalToRank(k)}{Card.OrdinalToRank(l)}{Card.OrdinalToRank(m)}");
						}
		}

		protected IEnumerable<string> AllOnePair3()
		{
			for (int j = 1; j < 14; j++)
				for (int k = 1; k < 14; k++)
				{
					if (j == k) continue;
					yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(k)}";
				}
		}

		protected IEnumerable<string> AllTwoPairs()
		{
			for (int j = 1; j < 13; j++)
				for (int k = j + 1; k < 14; k++)
					for (int l = 1; l < 14; l++)
					{
						if (j == k || l == j || k == l) continue;
						yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(k)}{Card.OrdinalToRank(k)}{Card.OrdinalToRank(l)}";
					}
		}

		public IEnumerable<string> AllThreeOfKind3()
		{
			for (int j = 1; j < 14; j++)
			{
				yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}";
			}
		}

		public IEnumerable<string> AllThreeOfKind5()
		{
			for (int j = 1; j < 14; j++)
				for (int k = 1; k < 14; k++)
					for (int l = k + 1; l < 14; l++)
					{
						if (j == k || j == l || k == l) continue;
						yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(k)}{Card.OrdinalToRank(l)}";
					}
		}

		public IEnumerable<string> AllStraightSorted()
		{
			for (int j = 1; j < 10; j++)
			{
				yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j + 1)}{Card.OrdinalToRank(j + 2)}{Card.OrdinalToRank(j + 3)}{Card.OrdinalToRank(j + 4)}";
			}

			yield return $"{Card.OrdinalToRank(10)}{Card.OrdinalToRank(11)}{Card.OrdinalToRank(12)}{Card.OrdinalToRank(13)}{Card.OrdinalToRank(1)}";
		}

		public IEnumerable<string> AllFullHouse()
		{
			for (int j = 1; j < 14; j++)
				for (int k = 1; k < 14; k++)
				{
					if (j == k) continue;
					yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(k)}{Card.OrdinalToRank(k)}";
				}
		}

		public IEnumerable<string> AllFourOfAKind()
		{
			for (int j = 1; j < 14; j++)
				for (int k = 1; k < 14; k++)
				{
					if (j == k) continue;
					yield return $"{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(j)}{Card.OrdinalToRank(k)}";
				}
		}

	}
}

