////////////////////////////////////////////////////////////////////////////////
//
// Class: FUNCTIONS
//  Contains general functions used througout the solution
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

        //
        // Trains a neural network via particle swarm
        public static Swarm RunParticleSwarm(ref Swarm swarm)
        {
            Stopwatch stopwatch = new Stopwatch(); ;
            Console.Write("Training neural network via particle swarm");
            for (int i = 1; i <= ITERATIONS; i++)
            {
                // Get start time
                if (i == 1)
                    stopwatch.Start();

                // Write progress
                if (i % (ITERATIONS / 10) == 0)
                {
                    Console.Write(100 * i / ITERATIONS);
                    if (i != ITERATIONS)
                        Console.Write(", ");
                    else
                        Console.WriteLine();
                }
                // Iterate
                swarm.GetParticleFitnesses(i);
                swarm.MoveParticles();
                if (swarm.AverageMovement.Last() < MIN_MOVEMENT)
                    swarm.ResetParticles();

                // Get stop time and estimate total runtime
                if (i == 5)
                {
                    stopwatch.Stop();
                    Console.WriteLine(", expected runtime: {0} s", ITERATIONS * stopwatch.ElapsedMilliseconds / 5000);
                }
            }
            Console.WriteLine();
            return swarm;
        }

        //
        // Reads in a .csv file with the option to remove the header
        public static string[][] ReadCSV(string fileName, bool hasHeader)
        {
            List<string[]> dataList = new List<string[]>();
            using (TextFieldParser parser = new TextFieldParser(fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                string[] headers;
                if (hasHeader)  // remove headers if they exist
                    headers = parser.ReadFields();
                while (!parser.EndOfData)
                {
                    // Processing row
                    string[] fields = parser.ReadFields();
                    dataList.Add(fields);
                }
            }
            return dataList.ToArray();
        }

        //
        // Reads in a network saved to file
        public static Neural_Network RememberNetwork(string fileName)
        {
            string[][] allData = ReadCSV(fileName, false);     // read in data

            // Set Layer Sizes
            int[] LayerSizes = new int[allData[0].Length];
            for (int i = 0; i < allData[0].Length; i++)
            {
                string[] thisLayerSize = allData[0];
                LayerSizes[i] = (int)Convert.ToDouble(thisLayerSize[i]);
            }
            // Set Opponent Data Info
            bool[] UseOpponent = new bool[allData[1].Length];
            for (int i = 0; i < allData[1].Length; i++)
            {
                string[] thisUseOpponent = allData[1];
                UseOpponent[i] = (thisUseOpponent[i] == "True") ? true : false;
            }
            // Set Offense Data Info
            bool[] UseOffense = new bool[allData[2].Length];
            for (int i = 0; i < allData[2].Length; i++)
            {
                string[] thisUseOffense = allData[2];
                UseOffense[i] = (thisUseOffense[i] == "True") ? true : false;
            }
            // Set Input Info
            int[] InputStats = new int[allData[3].Length];
            for (int i = 0; i < allData[3].Length; i++)
            {
                string[] thisInputStats = allData[3];
                InputStats[i] = (int)Convert.ToDouble(thisInputStats[i]);
            }
            // Set Output Info
            int[] OutputStats = new int[allData[4].Length];
            for (int i = 0; i < allData[4].Length; i++)
            {
                string[] thisOutputStats = allData[4];
                OutputStats[i] = (int)Convert.ToDouble(thisOutputStats[i]);
            }

            // Set network activation function and initialize network to 0
            int actFunction = (int)Convert.ToDouble(allData[5][0]);
            Neural_Network outputNetwork = new Neural_Network(LayerSizes, InputStats, OutputStats, UseOpponent, UseOffense, actFunction, 0);

            // Use data to set weights within network
            int currLayer = 0, currNode = 0;
            for (int i = 5; i < allData.Length; i++)             // the number of lines
            {
                for (int j = 0; j < allData[i].Length; j++)      // the number of weights in this node
                {
                    string[] weights = allData[i];
                    double weight = Convert.ToDouble(weights[j]);
                    outputNetwork.Layers[currLayer].Nodes[currNode].Weights[j] = weight;    // set weight
                }
                currNode++;
                if (currNode == outputNetwork.Layers[currLayer].Nodes.Length)
                {
                    currNode = 0;
                    currLayer++;
                }
            }

            return outputNetwork;
        }

        //
        // Converts an array of strings to a double array
        public static double[][] ConvertStringToDouble(string[][] strArray)
        {
            List<double[]> dblArray = new List<double[]>();
            foreach (string[] row in strArray)
            {
                List<double> dblRow = new List<double>();
                foreach (string cell in row)
                {
                    dblRow.Add(Convert.ToDouble(cell));
                }
                dblArray.Add(dblRow.ToArray());
            }
            return dblArray.ToArray();
        }

        //
        // Returns the adjusted rushing yards per attempt
        public static double AdjRushPerAtt(double[] data)
        {
            if (data[RUSH_ATT] == 0)        // no rush attempts
                return 0;
            else
                return (data[RUSH_YARD] + 20 * data[RUSH_TD] + 9 * data[FIRST_DOWN_RUSH]) / data[RUSH_ATT];
        }

        //
        // Returns the adjusted passing yards per attempt
        public static double AdjPassPerAtt(double[] data)
        {
            if (data[PASS_ATT] == 0)        // no pass attempts
                return 0;
            else
                return (data[PASS_YARD] + 20 * data[PASS_TD] - 45 * data[PASS_INT]) / data[PASS_ATT];
        }

        //
        // Returns the pythagorean expectation given RS and RA
        public static double GetPythagExp(double RS, double RA)
        {
            return Math.Pow(RS, PY_EXP) / (Math.Pow(RA, PY_EXP) + Math.Pow(RS, PY_EXP));
        }

        //
        // Returns true if this game is acceptable for use
        public static bool UseGame(Game G)
        {
            if (G.Home.Conference.Division == "FCS" || G.Visitor.Conference.Division == "FCS")
                return false;
            return true;
        }
    }
}
