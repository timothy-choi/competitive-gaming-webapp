namespace CompetitiveGamingApp.Controllers;

using System;
using System.Collections.Generic;

public class PlayerComparer : IComparer<Dictionary<string, object>> {
    private readonly List<string> sortKeys;

    public PlayerComparer(List<string> sortKeys) {
        this.sortKeys = sortKeys;
    }
    public int Compare(Dictionary<string, object> a, Dictionary<string, object> b) {
        bool calculated = false;
        foreach (var k in sortKeys) {
            if (k == "playerName" || k == "playerId") {
                continue;
            }
            else if ((k == "wins" || k == "loses" || k == "ties") && !calculated) {
                int res = CompareRecords(a, b);
                if (res != 0) {
                    return res;
                }
                calculated = true;
            }
            else if ((k != "wins" && k != "loses" && k != "ties")) {
                if (a[k].GetType() == typeof(int)) {
                    int one = Convert.ToInt32(a[k]);
                    int two = Convert.ToInt32(b[k]);
                    var res = one.CompareTo(two);
                    if (res != 0) {
                        return res;
                    }

                }
                else {
                    double one = Convert.ToInt32(a[k]);
                    double two = Convert.ToInt32(b[k]);
                    var res = one.CompareTo(two);
                    if (res != 0) {
                        return res;
                    }
                }
            }
        }
        return 0;
    }

    public int CompareRecords(Dictionary<string, object> a, Dictionary<string, object> b) {
        double aWinPer = findWinPercentage((int) a["wins"], (int) a["losses"], (int) a["draws"]);
        double bWinPer = findWinPercentage((int) b["wins"], (int) b["losses"], (int) b["draws"]);

        return aWinPer.CompareTo(bWinPer);
    }

    public double findWinPercentage(int wins, int losses, int draws) {
        int allGames = wins + losses + draws;
        return allGames == 0 ? 0 : (double) wins / allGames;
    }
}