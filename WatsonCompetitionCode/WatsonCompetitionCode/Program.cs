using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning;

namespace WatsonCompetitionCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Util utility = new Util();
            Dictionary<int, Canidate> dict = utility.csvReader("TGMC training-sample.csv");
            utility.fileWriter(dict,"test.txt");
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
                    if (values[i] == "true" || values[i] == "false")
                    {
                        if (values[i] == "true") canidate.isTrue = true;
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
        public void removeExtraneouData(Dictionary<int, Canidate> canidates)
        {
            List<List<float>> columns = new List<List<float>>();
            int columnCount = canidates.Values.First().featuresRating.Count();
            for (int k = 0; k < columnCount; k++)
            {
                columns.Add(new List<float>());
            }
            foreach (KeyValuePair<int, Canidate> pair in canidates)
            {
                Canidate c = pair.Value;
                int length = c.featuresRating.Count;
                for (int i = 0; i < length; i++)
                {
                    columns[i].Add(c.featuresRating[i]);
                }
            }
            List<int> removeIndex = new List<int>();
            
            for (int k = 0; k < columnCount-1;k++)
            {
                for (int i = k + 1; i < columnCount; i++)
                {
                    if (columns[k].Sum() == columns[i].Sum())
                    {
                        for (int j = 0; j < columns[k].Count; j++)
                        {
                            if (columns[k][j] != columns[i][j])
                            {
                                removeIndex.Insert(0, i);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
