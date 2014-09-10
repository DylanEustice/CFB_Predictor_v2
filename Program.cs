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

            // END
            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();
        }
    }
}
