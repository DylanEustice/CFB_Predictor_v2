////////////////////////////////////////////////////////////////////////////////
//
// Class: NODE
//  Representation of a node in an ANN (artificial neural network)
//
// Author:          Dylan Eustice
// Date Created:    9/3/2014
// Last Edited:     9/3/2014
//
////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFB_Predictor_v2
{
    public class Node
    {
        public double Value;
        public double[] Weights;
        public int ActFunction;

        //
        // Constructor
        public Node(int nWeights, int actFunction)
        {
            Value = 0;
            ActFunction = actFunction;
            Weights = new double[nWeights];
        }

        //
        // Uses the node value as an input to the activation function
        public void RunActivationFunc()
        {
            // Activation function
            switch (ActFunction)
            {
                case Program.HYP_TAN:
                    Value = (Math.Exp(Program.HYP_SCALE * Value) - 1) / (Math.Exp(Program.HYP_SCALE * Value) + 1);
                    break;
                case Program.LINEAR:
                    Value = Program.LINEAR_SCALE * Value;
                    break;
                case Program.EXPONENTIAL:
                    Value = 1 / (1 + Math.Exp(-Program.EXP_SCALE * Value));
                    break;
                default:
                    Value = 1 * Value;
                    break;
            }
        }
    }
}