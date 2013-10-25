using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
 



namespace WatsonCompetitionCode
{
    
    public class LogisiticRegression
    {
        private Dictionary<int, Canidate> trainingDataSet;
        public DenseVector theta;
        private DenseVector J;
        private DenseMatrix x;
        private DenseVector y;
        private DenseMatrix hessianResult;
        private int m;
        private int n;
        public const int MAX_ITERATIONS = 7;
        public int timesNonInvertable = 0;
        public double nonInvertableModifier = 0.0001;

        private LogisiticRegression()
        {
            
        }
        

        public LogisiticRegression(Dictionary<int,Canidate> data)
        {
            this.trainingDataSet = data;
            int numberFeatures = trainingDataSet[15].featuresRating.Count;

            //Initialize xdata matrix
            DenseVector[] xdata;
            xdata = new DenseVector[trainingDataSet.Count];
            //xdata[0] = new DenseVector(numberFeatures,1); 
            int k = 0;
   
            //intialize y data
            List<double> ydata = new List<double>();
            
            //fill x and y data from dictionary
            foreach (KeyValuePair<int,Canidate> canidate in trainingDataSet)
            {
                List<double> intermediate =  canidate.Value.featuresRating.ToList();
                intermediate.Insert(0, 1);
                xdata[k] = new DenseVector(intermediate.ToArray());
                if (canidate.Value.isTrue) ydata.Add(1);
                else ydata.Add(0);
                k++;
            }

            //populate fields with data
            x = DenseMatrix.OfRowVectors(xdata);
            y = new DenseVector(ydata.ToArray());

            m = x.RowCount;
            n = x.ColumnCount-1;

            //Intialize fitting parameters, theta
            theta = new DenseVector(n+1,0);
           
            this.writeToFile();
        }

        //Creates an AI That only has Theta, cannon train or anything.
        /// <summary>
        /// Creates an LogrithmicRegression based on previous training data. Only intilializes theta vector, all others null. So only probabilty function works
        /// </summary>
        /// <param name="path">The Path to the AI file. Defaults to "LogisticRegressionAi.txt"</param>
        public LogisiticRegression(String path = "LogisticRegressionAi.txt")
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(path);
            String line = reader.ReadLine();
            String[] values = line.Split(',');
            List<Double> thetaL = new List<double>();
            foreach (String value in values)
            {
                if (value == "") break;
                thetaL.Add(Convert.ToDouble(value));
            }
            
            reader.Close();
            this.theta = new DenseVector(thetaL.ToArray());    
            Console.WriteLine("Successfuly Restored old AI");
        }

        public DenseVector convergenceValue()
        {
            return J;
        }

        private DenseVector sigmoid(DenseVector z)
        {
            List<double> vector = new List<double>();
            foreach (double element in z)
            {
                vector.Add((double)(1.0 / (1.0 + Math.Exp(-element))));
            }
            return new DenseVector(vector.ToArray());
        }
        private double sigmoid(double z)
        {
            return 1.0 / (1.0 + Math.Exp(-z));
        }
        //Probability Function
        public double probability(List<double> features)
        {
            List<double> list = features.ToList();
            list.Insert(0, 1);
            DenseVector feat = new DenseVector(list.ToArray());
            double z = feat * theta;
            return sigmoid(z);
        }
        private DenseVector gradient(DenseMatrix xMat, DenseVector hVec, DenseVector yVec)
        {
            DenseVector grad = (DenseVector)(((double) 1.0 / m) * (xMat.Transpose()) * (hVec - yVec));

            return grad;
        }
        private DenseMatrix hessian(DenseMatrix xMat, DenseVector hVec)
        {
            DenseMatrix result;
            DiagonalMatrix diag = DiagonalMatrix.Identity(hVec.Count);
            diag.SetDiagonal(hVec);
            DiagonalMatrix diagMinus1 = DiagonalMatrix.Identity(hVec.Count);
            diagMinus1.SetDiagonal(1-hVec);
            result = (DenseMatrix) ((xMat.Transpose().Divide(m)) * diag * diagMinus1 * xMat);
            Console.WriteLine("Computed Hessian");
            return result;
        }
        public void train(int iterations = MAX_ITERATIONS)
        {
            J = new DenseVector(iterations, 0);
            theta = new DenseVector(n + 1, 0);
            for (int k = 0; k < iterations; k++)
            {
                DenseVector z = x * theta;
                DenseVector h = sigmoid(z);
                DenseVector grad = gradient(x, h, y);
                DenseMatrix H = hessian(x, h);

                //Calculate J for testing convergence
                J[k] = (double) (y.Negate().PointwiseMultiply((DenseVector) log(h)) - (1-y).PointwiseMultiply( (DenseVector) log((DenseVector)(1-h)))).Sum()/m;
               
                //Compute Theta
                theta = (DenseVector)(theta - H.Inverse() * grad);

                //Check for non invertable Hessian Matrix, if true, re-train with diagonals have slight value added
                if (Double.IsNaN(theta[0]))
                {
                    DenseVector xDiag = (DenseVector) x.Diagonal();
                    x.SetDiagonal(xDiag + nonInvertableModifier);
                    timesNonInvertable++;
                    Console.Write("Unable to Compute Inverse, adding to Diagonal");
                    Console.WriteLine();
                    train(iterations);
                    break;
                }
            }
        }
        private DenseVector log(DenseVector vec)
        {
            List<double> result = new List<double>();
            foreach (double element in vec)
            {
                result.Add((double)Math.Log(element));
            }
            return new DenseVector(result.ToArray());
        }
        public void writeToFile(String path = "LogisticRegressionAi.txt")
        {
            List<double> thetaL = theta.ToList();
            StringBuilder s = new StringBuilder();
            foreach (double val in thetaL)
            {
                s.Append(val);
                s.Append(",");
            }
            

            var writer = new System.IO.StreamWriter(path);
            writer.WriteLine(s.ToString());
            writer.Close();
        }
        
        
    }
    
}

