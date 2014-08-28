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
        // Constructor
        public Team(int code, string name, Conference conference)
        {
            Code = code;
            Name = name;
            Conference = conference;
        }

        //
        // Adds all games to the game list
        public void GetGames(List<Game> games)
        {
            foreach (Game G in games)
                if (G.Home == this || G.Visitor == this)
                    Games.Add(G);
        }

        //
        // Returns the season average of a stat
        public double GetSeasonAverage(int stat)
        {
            return GetSeasonTotal(stat) / Games.Count;
        }

        //
        // Returns the season total of a stat
        public double GetSeasonTotal(int stat)
        {
            double total = 0;
            foreach (Game G in Games)
            {
                if (ThisTeamHome(G))
                    total += G.HomeData[stat];
                else
                    total += G.VisitorData[stat];
            }
            return total;
        }

        //
        // Returns this team from the game (home/visitor)
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
