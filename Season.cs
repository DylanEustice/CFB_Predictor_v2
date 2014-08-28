////////////////////////////////////////////////////////////////////////////////
//
// Class: SEASON
//  Contains all data for a season of college football
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
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Diagnostics;

namespace CFB_Predictor_v2
{
    public class Season
    {
        public int Year;
        public List<Team> Teams = new List<Team>();
        public List<Game> Games = new List<Game>();
        public List<Conference> Conferences = new List<Conference>();
        public List<Season> PastSeasons = new List<Season>();
        public double[][] TeamGameStats;

        //
        // Constructor
        public Season(int year, List<Season> pastSeasons)
        {
            Year = year;
            PastSeasons = pastSeasons;
            Console.WriteLine("Reading data and building teams from {0}\n", Year);

            // Read in team-game-statistics.csv data file
            string yearString = Year.ToString();
            string pathName = "../../Statistics" + "/" + yearString + "/";
            string fileName = "team-game-statistics.csv";
            string[][] teamGameStatsStr = Program.ReadCSV(pathName, fileName, true);
            TeamGameStats = Program.ConvertStringToDouble(teamGameStatsStr);

            // Read in conference data from file and build conference list
            fileName = "conference.csv";
            string[][] confData = Program.ReadCSV(pathName, fileName, true);
            BuildConferenceList(confData);

            // Read in team data from file and build team list
            fileName = "team.csv";
            string[][] teamData = Program.ReadCSV(pathName, fileName, true);
            BuildTeamList(teamData);

            // Build games
            BuildGameList();
            foreach (Team T in Teams)
                T.GetGames(Games);
        }

        //
        // Builds the conference list from raw conference.csv input
        public void BuildConferenceList(string[][] confData)
        {
            foreach (string[] row in confData)
            {
                int code = (int)Convert.ToDouble(row[0]);
                string name = row[1];
                string div = row[2];
                Conference newConference = new Conference(code, name, div);
                Conferences.Add(newConference);
            }
        }

        //
        // Builds the team list from raw team.csv input
        public void BuildTeamList(string[][] teamData)
        {
            foreach (string[] row in teamData)
            {
                int code = (int)Convert.ToDouble(row[0]);
                string name = row[1];
                int conf = (int)Convert.ToDouble(row[2]);
                int i = 0;  // find this team's conference
                while (Conferences[i].Code != conf)
                    i++;
                Team newTeam = new Team(code, name, Conferences[i]);
                Teams.Add(newTeam);
            }
        }

        //
        // Builds the game list from TeamGameStats
        public void BuildGameList()
        {
            foreach (double[] row in TeamGameStats)
            {
                bool alreadyAdded = false;
                foreach (Game G in Games)   // check if this game has been added
                    if (G.Code == row[Program.GAME_CODE])
                    {
                        alreadyAdded = true;
                        break;
                    }
                if (!alreadyAdded)
                    Games.Add(new Game(row, this));
            }
        }
    }
}
