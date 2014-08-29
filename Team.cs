////////////////////////////////////////////////////////////////////////////////
//
// Class: TEAM
//  Contains all data for a team from a specific season of college football
//
// Author:          Dylan Eustice
// Date Created:    8/27/2014
// Last Edited:     8/27/2014
//
////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFB_Predictor_v2
{
    public class Team
    {
        public int Code;
        public string Name;
        public Conference Conference;
        public List<Game> Games = new List<Game>();

        //
        // Blank constructor
        public Team()
        {
        }

        //
        // Constructor
        public Team(int code, string name, Conference conference)
        {
            Code = code;
            Name = name;
            Conference = conference;
            if (!IsInConference())
                Conference.Teams.Add(this);
        }

        //
        // Sorts the games in order of date
        public void InsertGame(Game G)
        {
            int index = 0;
            while (index < Games.Count)
            {
                if (Games[index].Date > G.Date)
                {
                    Games.Insert(index, G);
                    return;
                }
                else
                    index++;
            }
            Games.Add(G);
        }

        //
        // Checks if this team is already added to the conference
        public bool IsInConference()
        {
            foreach (Team T in Conference.Teams)
                if (T == this)
                    return true;
            return false;
        }

        //
        // Returns a list of data arrays for games previous to the input (from this season)
        public List<double[]> ReturnSimpleLists(Game game)
        {
            List<double[]> returnList = new List<double[]>();
            foreach (Game G in Games)
            {
                if (Program.UseGame(G) && game.Date > G.Date)
                {
                    double[] dataTGS = new double[Program.SIMPLE_PTS];
                    bool isHome = ThisTeamHome(G);  // get TGS stats out of this game and add to the list
                    for (int i = 0; i < dataTGS.Length; i++)
                        dataTGS[i] = isHome ? G.HomeData[i] : G.VisitorData[i];
                    returnList.Add(dataTGS);
                }
            }
            return returnList;
        }

        //
        // Returns a list of data arrays for games previous to the input (from this season)
        public List<double[]> ReturnOppSimpleLists(Game game)
        {
            List<double[]> returnList = new List<double[]>();
            foreach (Game G in Games)
            {
                if (Program.UseGame(G) && game.Date > G.Date)
                {
                    double[] dataTGS = new double[Program.SIMPLE_PTS];
                    bool isHome = !ThisTeamHome(G);  // get TGS stats out of this game and add to the list
                    for (int i = 0; i < dataTGS.Length; i++)
                        dataTGS[i] = isHome ? G.HomeData[i] : G.VisitorData[i];
                    returnList.Add(dataTGS);
                }
            }
            return returnList;
        }

        //
        // Returns the season average of a stat
        public double GetSeasonAverage(int stat)
        {
            double nGames = 0;
            double total = GetSeasonTotal(stat, ref nGames);
            if (nGames == 0)
                return 0;
            else
                return total / nGames;
        }

        //
        // Returns the season average of a stat for this team's opponent
        public double GetOppSeasonAverage(int stat)
        {
            double nGames = 0;
            double total = GetOppSeasonTotal(stat, ref nGames);
            if (nGames == 0)
                return 0;
            else
                return total / nGames;
        }

        //
        // Returns the season total of a stat
        public double GetSeasonTotal(int stat, ref double nGames)
        {
            double total = 0;
            foreach (Game G in Games)
            {
                if (!Program.UseGame(G))
                    continue;
                nGames++;
                if (ThisTeamHome(G))
                    total += G.HomeData[stat];
                else
                    total += G.VisitorData[stat];
            }
            return total;
        }

        //
        // Returns the season total of a stat for this team's opponent
        public double GetOppSeasonTotal(int stat, ref double nGames)
        {
            double total = 0;
            foreach (Game G in Games)
            {
                if (!Program.UseGame(G))
                    continue;
                nGames++;
                if (ThisTeamHome(G))
                    total += G.VisitorData[stat];
                else
                    total += G.HomeData[stat];
            }
            return total;
        }

        //
        // Returns true if this team is home
        public bool ThisTeamHome(Game G)
        {
            if (G.Home == this)
                return true;
            else if (G.Visitor == this)
                return false;
            else
            {
                Console.WriteLine("WARNING: Team not found!");
                Console.WriteLine("         Function: ThisTeamHome()");
                Console.WriteLine("         Team: {0}\n", Name);
                return false;
            }
        }
    }
}
