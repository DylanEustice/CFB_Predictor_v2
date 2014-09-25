////////////////////////////////////////////////////////////////////////////////
//
// Class: SWARM
//  A swarm of particles used to find optimal neural network
//
// Author:          Dylan Eustice
// Date Created:    9/24/2014
// Last Edited:     9/24/2014
//
////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFB_Predictor_v2
{
    public class Swarm
    {
        // Global best
        public double GlobalBest = double.MaxValue;
        public List<double> GlobalBestHistory = new List<double>();
        public List<double> GlobalBestUpdateIteration = new List<double>();
        public Neural_Network bestNetwork;

        // Movement parameters
        public double Momentum;
        public double GlobalWeight;
        public double PersonalWeight;
        public List<double> AverageMovement = new List<double>();

        // Training structures
        public Particle[] Particles;
        public Game[] TrainGames;
        public Game[] TestGames;
        public int Resets = 0;

        // Neural network stuff
        public int[] LayerSizes;
        public bool[] UseOpponent;
        public bool[] UseOffense;
        public int[] InputStats;
        public int[] OutputStats;
        public int ActFunction = Program.USE_ACT;
        Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

        //
        // Constructor
        public Swarm(int nParticles, double momentum, double weightGlobal, double weightPersonal, 
            int[] LayerInfo, int[] inputTypes, int[] outputTypes, bool[] inUseOpponent, bool[] inUseOffense,
            Game[] trainGames, Game[] testGames)
        {
            // Set parameters
            Momentum = momentum;
            GlobalWeight = weightGlobal;
            PersonalWeight = weightPersonal;
            LayerSizes = LayerInfo;
            InputStats = inputTypes;
            OutputStats = outputTypes;
            UseOpponent = inUseOpponent;
            UseOffense = inUseOffense;
            TrainGames = trainGames;
            TestGames = testGames;

            // Initialize particles
            Particles = new Particle[nParticles];
            for (int i = 0; i < nParticles; i++)
                Particles[i] = new Particle(LayerSizes, InputStats, OutputStats, UseOpponent, UseOffense, ActFunction, random);

            // Get fitnesses and find best initial particle
            GetParticleFitnesses(-1);
        }

        //
        // Updates all fitnesses and checks for new global best
        public void GetParticleFitnesses(int it)
        {
            for (int i = 0; i < Particles.Length; i++)
            {
                Particles[i].GetFitness(TrainGames);   // get fitness
                if (Particles[i].PersonalBest < GlobalBest)                 // check for new global best
                {
                    GlobalBest = Particles[i].PersonalBest;
                    GlobalBestHistory.Add(GlobalBest);
                    GlobalBestUpdateIteration.Add(it);
                    bestNetwork = Particles[i].bestNetwork.SetWeights();
                }
            }
        }

        //
        // Adjusts the weights of all particles in this swarm
        public void MoveParticles()
        {
            double tot = 0;
            for (int i = 0; i < Particles.Length; i++)
            {
                Particles[i].MoveParticle(Momentum, GlobalWeight, PersonalWeight, bestNetwork);
                tot += Particles[i].AverageMovement;
            }
            AverageMovement.Add(tot / Particles.Length);
        }

        //
        // Resets the particle weights when there is too little movement
        public void ResetParticles()
        {
            for (int i = 0; i < Particles.Length; i++)
            {
                Particles[i].currNetwork = new Neural_Network(LayerSizes, InputStats, OutputStats, UseOpponent, UseOffense, ActFunction, random, Resets);
                Particles[i].stepNetwork = new Neural_Network(LayerSizes, InputStats, OutputStats, UseOpponent, UseOffense, ActFunction, random, 0);
            }
            Resets++;
        }

        //
        // Get MSE of global best network to test data
        public double BestTestFitness()
        {
            // Get error
            double error = 0;
            foreach (Game G in TestGames)
            {
                double homePts = G.PredictTeamPoints(bestNetwork, true);
                double visitPts = G.PredictTeamPoints(bestNetwork, false);
                error += Math.Abs((homePts - G.HomeData[Program.POINTS]) * (homePts - G.HomeData[Program.POINTS]));
                error += Math.Abs((visitPts - G.VisitorData[Program.POINTS]) * (visitPts - G.VisitorData[Program.POINTS]));
            }
            return error / TestGames.Length;
        }

        //
        // Get MSE of global best network to training data
        public double BestTrainingFitness()
        {
            // Get error
            double error = 0;
            foreach (Game G in TrainGames)
            {
                double homePts = G.PredictTeamPoints(bestNetwork, true);
                double visitPts = G.PredictTeamPoints(bestNetwork, false);
                error += Math.Abs((homePts - G.HomeData[Program.POINTS]) * (homePts - G.HomeData[Program.POINTS]));
                error += Math.Abs((visitPts - G.VisitorData[Program.POINTS]) * (visitPts - G.VisitorData[Program.POINTS]));
            }
            return error / TestGames.Length;
        }

        //
        // Get MSE of global best network to all data
        public double BestTotalFitness()
        {
            // Test games
            double error = 0;
            foreach (Game G in TestGames)
            {
                double homePts = G.PredictTeamPoints(bestNetwork, true);
                double visitPts = G.PredictTeamPoints(bestNetwork, false);
                error += Math.Abs((homePts - G.HomeData[Program.POINTS]) * (homePts - G.HomeData[Program.POINTS]));
                error += Math.Abs((visitPts - G.VisitorData[Program.POINTS]) * (visitPts - G.VisitorData[Program.POINTS]));
            }
            // Training games
            foreach (Game G in TrainGames)
            {
                double homePts = G.PredictTeamPoints(bestNetwork, true);
                double visitPts = G.PredictTeamPoints(bestNetwork, false);
                error += Math.Abs((homePts - G.HomeData[Program.POINTS]) * (homePts - G.HomeData[Program.POINTS]));
                error += Math.Abs((visitPts - G.VisitorData[Program.POINTS]) * (visitPts - G.VisitorData[Program.POINTS]));
            }
            return error / (TestGames.Length + TrainGames.Length);
        }
    }
}
