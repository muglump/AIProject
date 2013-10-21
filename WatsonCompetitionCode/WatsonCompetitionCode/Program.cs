using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WatsonCompetitionCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Util utility = new Util();
            Dictionary<int, Canidate> dict = utility.csvReader("TGMC training-sample.csv");
            utility.fileWriter(dict,"test.txt");
            //Decision Trees need to be before this preprocessor.
            utility.removeExtraneouData(dict);
            //Console.ReadLine();
        }

       
    }
    class Canidate
    {
        public int rowNumber;
        public float questionNumber;
        public List<float> featuresRating;
        public Boolean isTrue;
        public Boolean givenAnswer;

        public Canidate()
        {
            rowNumber = 0;
            questionNumber = 0;
            featuresRating = new List<float>();
            isTrue = false;
            givenAnswer = false;
        }
    }
    class Util 
    {
         public Dictionary<int,Canidate> csvReader(string fileName)
        {
            var reader = new StreamReader(File.OpenRead(@fileName));
            List<string> lineRead = new List<string>();
            Dictionary<int,Canidate> canidates = new Dictionary<int,Canidate>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                Canidate canidate = new Canidate();
                canidate.rowNumber = Convert.ToInt32(values[0]);
                canidate.questionNumber = Convert.ToSingle(values[1]);
                for (int i = 2; i < values.Count(); i++)
                {
                    if (values[i] == "true" || values[i] == "false" || values[i] == "TRUE" || values[i] == "FALSE")
                    {
                        if (values[i] == "true" || values[i] == "TRUE") canidate.isTrue = true;
                        else canidate.isTrue = false;
                        canidate.givenAnswer = true;
                        break;
                    }
                    canidate.featuresRating.Add(Convert.ToSingle(values[i]));
                }
                canidates.Add(canidate.rowNumber, canidate);
            }
            return canidates;
        }
        public void fileWriter(Dictionary<int, Canidate> canidates,string fileName)
        {
            var writer = new StreamWriter(fileName);
            int i = 1;
            Canidate canidate = new Canidate();
            while (canidates.ContainsKey(i))
            {
                canidates.TryGetValue(i, out canidate);
                if (canidate.isTrue)
                {
                    writer.WriteLine(canidate.questionNumber.ToString());
                }
                i++;
            }
            writer.Close();
        }
        public List<int> removeExtraneouData(Dictionary<int, Canidate> canidates)
        {
            //intialize size of array of columns
            List<List<float>> columns = new List<List<float>>();
            int columnCount = canidates.Values.First().featuresRating.Count();
            for (int k = 0; k < columnCount; k++)
            {
                columns.Add(new List<float>());
            }

            //Generate Array of Columns
            foreach (KeyValuePair<int, Canidate> pair in canidates)
            {
                Canidate c = pair.Value;
                int length = c.featuresRating.Count;
                for (int i = 0; i < length; i++)
                {
                    columns[i].Add(c.featuresRating[i]);
                }
            }

            //Add columns to remove that contain same data
            List<int> removeData = new List<int>();
            for (int k = 0; k < columnCount-1;k++)
            {
                for (int i = k + 1; i < columnCount; i++)
                {
                    if (columns[k].Sum() == columns[i].Sum())
                    {
                        bool isRemove = true;
                        for (int j = 0; j < columns[k].Count; j++)
                        {
                            if (columns[k][j] != columns[i][j])
                            {
                                isRemove = false;
                                break;
                            }
                        }
                        if (isRemove && !removeData.Contains(i))
                        {
                            removeData.Add(i);
                        }
                    }
                }
            }
            //Add columns that have all same values (without doubling up on previous addition)
            for (int k = 0; k < columns.Count; k++)
            {
                float firstElement = columns[k][0];
                bool isRemove = true;
                foreach (float feature in columns[k])
                {
                    if (firstElement != feature)
                    {
                        isRemove = false;
                        break;
                    }
                }
                if (isRemove && !removeData.Contains(k))
                {
                    removeData.Add(k);
                }
            }

            List<int> result = removeData.ToList();

            while (removeData.Count >0)
            {
                foreach (KeyValuePair<int, Canidate> pair in canidates)
                {
                    pair.Value.featuresRating.RemoveAt(removeData.Max());
                }
                columns.RemoveAt(removeData.Max());
                //Console.WriteLine("Removed Column " + removeData.Max());
                removeData.Remove(removeData.Max());
            }
            List<float> maxValues = new List<float>();
            foreach (List<float> column in columns)
            {
                float absMax;
                if (Math.Abs(column.Min()) > Math.Abs(column.Max())) absMax = Math.Abs(column.Min());
                else absMax = Math.Abs(column.Max());
                if (absMax == 0.0) absMax = (float) 1.0;
                maxValues.Add(absMax);
            }

            for (int k = 0; k < maxValues.Count; k++)
            {
                foreach (KeyValuePair<int, Canidate> canidate in canidates)
                {
                    canidate.Value.featuresRating[k] = (canidate.Value.featuresRating[k] / maxValues[k]);
                }
            }
            return result;
        }
    }
}
