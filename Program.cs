////////////////////////////////////////////////////////////////////////////////
//
// COLLEGE FOOTBALL PREDICTION ALGORITHM (VERSION 2)
//  Using a neural networks trained with particle swarm to predict 
//  outcomes and scores of college football. This version is less of
//  a prototype and is intended to be cleaner and more robust than
//  version 1. 
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
    public partial class Program
    {
        static void Main(string[] args)
        {
            // Build all seasons
            List<Season> allSeasons = new List<Season>();
            for (int year = 2005; year <= 2013; year++)
            {
                Season newSeason = new Season(year, allSeasons);    // read new season
                allSeasons.Add(newSeason);
            }
            NormalizeSeasonMetrics(ref allSeasons);

            Neural_Network testNetwork = TrainNetwork(allSeasons);
            foreach (Game G in allSeasons[8].Games)
            {
                Console.WriteLine("=====================================================");
                Console.WriteLine(G.Home.Name);
                Console.WriteLine("Predicted: {0}", G.PredictTeamPoints(testNetwork, true));
                Console.WriteLine("   Actual: {0}\n", G.HomeData[POINTS]);
                Console.WriteLine(G.Visitor.Name);
                Console.WriteLine("Predicted: {0}", G.PredictTeamPoints(testNetwork, false));
                Console.WriteLine("   Actual: {0}", G.VisitorData[POINTS]);
                Console.WriteLine("=====================================================");
                Console.ReadLine();
            }

            // END
            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();
        }
    }
}
