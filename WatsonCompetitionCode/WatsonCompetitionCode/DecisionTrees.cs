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

        public DecisionTrees(Dictionary<int, Canidate> canidates, int branchesPerNode)
        {
            List<List<float>> rows = new List<List<float>>();
            List<bool> rowTruth = new List<bool>();
            //Pull apart the dictionary because its easier to work with this way.
            //Additionally fill a 2D List of all rows and columns, and a List of the row truths
            List<Canidate> listCandidates = new List<Canidate>();
            foreach (Canidate value in canidates.Values)
            {
                listCandidates.Add(value);
                rows.Add(value.featuresRating);
                rowTruth.Add(value.isTrue);
            }

            //We need to try and reduce the Branching factor, so we call the following function. Additionally we now will only have integers
            List<List<int>> newRows = reduceBranching(rows, branchesPerNode);

            //Now we are done with the preprocessing and need to actually start building the items we need to create the id3Tree
            //Convert to int[][] and bool[] because that is what the tree needs to work
            
            List<int[]> transform = new List<int[]>();
            foreach(List<int> list in newRows){
                transform.Add(list.ToArray());
            }
            int[][] inputs = transform.ToArray();

            List<int> truth = new List<int>();
            foreach (bool value in rowTruth)
            {
                if (value == true)
                {
                    truth.Add(1);
                }
                else
                {
                    truth.Add(0);
                }
            }

            int[] outputs = truth.ToArray();
            


            //number of possible outputs form the tree
            int classCount = 2;

            //Get the number of possible values for each column
            List<int> colValCount = getColValueCount(newRows);
            int count = colValCount.Count;
            Accord.MachineLearning.DecisionTrees.DecisionVariable[] attribute;
            attribute = new Accord.MachineLearning.DecisionTrees.DecisionVariable[count];
            for (int i = 0; i < colValCount.Count; i++)
            {
                attribute[i] = new Accord.MachineLearning.DecisionTrees.DecisionVariable(i.ToString(), colValCount[i]);
            }


            //Now we are ready to actually start making the tree
            Accord.MachineLearning.DecisionTrees.DecisionTree tree = new Accord.MachineLearning.DecisionTrees.DecisionTree(attribute, classCount);

            //Make a new ID3 algorithm
            Accord.MachineLearning.DecisionTrees.Learning.ID3Learning id3learning = new ID3Learning(tree);

            //Run the training data
            id3learning.Run(inputs, outputs);
        }

        //This method returns an array that contains the number of branches a column can has
        private List<int> getColValueCount(List<List<int>> cols)
        {
            List<int> columnCount = new List<int>();
            for (int i = 0; i < cols[0].Count; i++)
            {
                List<int> columnVals = new List<int>();
                for (int j = 0; j < cols.Count; j++)
                {
                    if (!columnVals.Contains(cols[j][i]))
                    {
                        columnVals.Add(cols[j][i]);
                    }
                }
                columnCount.Add(columnVals.Count);
            }
            return columnCount;
        }




        //This function takes a 2D list and reduces the variation in columns such that only and allowed number of values
        private List<List<int>> reduceBranching(List<List<float>> rows, int branchesPerNode)
        {
            List<List<int>> newRows = new List<List<int>>();
            //start at the first column and loop through the column values by row
            
            for(int i=0; i < rows[0].Count; i++){
                List<float> columnValues = new List<float>();
                for (int j = 0; j < rows.Count; j++)
                {
                    columnValues.Add(rows[j][i]);
                }
                //No that we have and array with all of the values for a column check the variation, if it is less then or equal to the allow amount we proceed,
                //otherwise we will need to normalize the values
                List<int> newColumn = new List<int>();
                if(nValues(columnValues, branchesPerNode))
                {
                    //Reduce the variation in values
                    newColumn = reduce(columnValues, branchesPerNode);
                }

                else
                {
                    foreach (float item in columnValues)
                    {
                        newColumn.Add(Convert.ToInt32(item));
                    }
                }
                newRows.Add(newColumn);
            }
            return newRows;

        }

        //This method reduces the number of values such that they are <= branches number of different values
        //To do this we sort the list, and split it into branches number of sublists.
        //The median is then found for each sublist, and values are assigned by the current values relationship to those values
        private List<int> reduce(List<float> column, int branches)
        {
            column.Sort();
            //Figure out how to divide the items, this may result in slightly unequal arrays but it happend to be the best way to divide 
            int divider = column.Count / branches - 1;
            List<List<float>> listOfSubList = new List<List<float>>();
            int lowerbound = 0;
            int upperbound = divider;
            int step = 0;
            //Break the list into the properly sized subLists
            while (step < branches)
            {
                if (step + 1 == branches)
                {
                    upperbound = column.Count;
                }
                List<float> sublist = new List<float>();
                for (int i = lowerbound; lowerbound <= upperbound; lowerbound++)
                {
                    sublist.Add(i);
                }
                listOfSubList.Add(sublist);
                lowerbound = upperbound + 1;
                upperbound += upperbound;
                step += 1;
            }

            //Find the median of each sublist
            List<float> medians = new List<float>();
            foreach (List<float> list in listOfSubList)
            {
                float value = 0;
                for (int k = 0; k < list.Count; k++)
                {
                    value += list[k];
                }
                value = value / list.Count;
                medians.Add(value);
            }

            //Now loop through all of the values in the columns and assing a value based off relationship to median values
            List<int> newColumn = new List<int>();
            foreach (float value in column)
            {
                for (int j = 0; j < medians.Count; j++)
                {
                    if (value < medians[j])
                    {
                        newColumn.Add(j);
                        break;
                    }
                    if (j + 1 == medians.Count)
                    {
                        if (value > medians[j])
                        {
                            newColumn.Add(j + 1);
                            break;
                        }
                    }
                   
                }
            }
            return newColumn;
        }



        //Checks the number of values in the List, if there are more then allowedNum returns true, otherwise false;
        private bool nValues(List<float> vals, int allowedNum)
        {
            List<float> values = new List<float>();
            foreach (float item in vals)
            {
                if (!values.Contains(item))
                {
                    values.Add(item);
                }
            }
            if (values.Count > allowedNum)
            {
                return true;
            }
            return false;
        }


            
    }
}
