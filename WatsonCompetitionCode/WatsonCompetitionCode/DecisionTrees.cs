using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord;
using System.Data;


namespace WatsonCompetitionCode
{
    class DecisionTrees<T>
    {
        public Accord.MachineLearning.DecisionTrees.DecisionTree decisionTree;
        public List<List<float>> possibleValuesPerColumn;

        public DecisionTrees(Dictionary<int, Canidate> canidates)
        {
            //Initialize a DataTable to hold all of the canidates 
            DataTable data = new DataTable("Training Data for DecisionTrees");
            List<Canidate> values = canidates.Values.ToList();
            List<int> columnHeaders = new List<int>();
            List<int[]> columnValues = new List<int[]>();
            //Just making some column headers because we have to for the Datatable
            int numColumns = values[0].featuresRating.Count;
            for (int i = 0; i < numColumns; i++)
            {
                columnHeaders.Add(i);
            }
            //For each canidate add their column values to the datatable
            foreach (Canidate value in values)
            {
                columnValues.Add(value.featuresRating.ToArray()); ;
                data.Rows.Add(value.featuresRating);
            }
            //This is the number of possible outcomes in this case either True or False
            int classCount = 2;
            //This section does twofold first it gets all of the possible values for each column. This will allow us to normalize the values for that column if we get values that
            //are outside of the given range. Second is it will allow us to number of values per column so that the tree can be constructed correctly. This information may come into play
            //later if we normalize the values
            for (int i = 0; i < numColumns; i++)
            {
                possibleValuesPerColumn.Add(getPossibleValue(i, values));
            }
            //create an array of attributes that 
            Accord.MachineLearning.DecisionTrees.DecisionVariable[] attributes = new Accord.MachineLearning.DecisionTrees.DecisionVariable[numColumns];
            for (int i = 0; i < numColumns; i++)
            {
                attributes[i] = new Accord.MachineLearning.DecisionTrees.DecisionVariable(columnHeaders.ToString(), possibleValuesPerColumn[i].Count);
            }
            decisionTree = new Accord.MachineLearning.DecisionTrees.DecisionTree(attributes, numColumns);

            var learning = new ID3Learning(decisionTree);
            learning.Run(columnValues.ToArray(), new int[]{0, 1});
            
            
            
        }


        private List<float> getPossibleValue(int index, List<Canidate> canidates)
        {
            List<float> possibleValues = new List<float>();
            foreach (Canidate canidate in canidates)
            {
                if(!(possibleValues.Contains(canidate.featuresRating[index]))){
                    possibleValues.Add(canidate.featuresRating[index]);
                }
            }
            return possibleValues;
        }
    }
}
