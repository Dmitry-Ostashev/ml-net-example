using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.Extensions.Configuration;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;

namespace MlNetExample
{
    class Program
    {
        private static DataManager dataManager;
        private static IEnumerable<IBetPoint> GetTrainData() {
            if(dataManager == null) {
                dataManager = new DataManager();
            }
            return dataManager.GenerateData();
        }
        private static void RunComparingTrain(List<BetPoint> predictionData) {
            MLContext mlContext = new MLContext(0);
            IDataView trainDataView = mlContext.Data.LoadFromEnumerable(GetTrainData());
            var pipeline = mlContext.Transforms
                .CopyColumns(outputColumnName: "Label", inputColumnName: "Gain")
                .Append(mlContext.Transforms.Concatenate("Features", 
                    "Koeff",
                    "Value",
                    "Minute",
                    "MaxDelta",
                    "Total",
                    "Delta",
                    "Period",
                    "KoefRatio"
                    ))
                .Append(mlContext.Regression.Trainers.LbfgsPoissonRegression());
            var model = pipeline.Fit(trainDataView);            
            
            predictionData[0].BetResult = 0;
            predictionData[1].BetResult = 0;
            predictionData[2].BetResult = 0;

            var predictionFunction = mlContext.Model.CreatePredictionEngine<BetPoint, BetPointPrediction>(model);
            var prediction = predictionFunction.Predict(predictionData[0]);
            Console.WriteLine(prediction.Gain);

            prediction = predictionFunction.Predict(predictionData[1]);
            Console.WriteLine(prediction.Gain);

            prediction = predictionFunction.Predict(predictionData[2]);
            Console.WriteLine(prediction.Gain);

        }
        private static void PrepareDb() {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true);

            IConfigurationRoot config = builder.Build();
            // string connectionString = "User=sa;Password=Qwerty123-;Pooling=false;Data Source=(local);Initial Catalog=ml-net-example";
            string connectionString = config.GetConnectionString("MySqlConnectionString");
            XPDictionary dict = new ReflectionDictionary();
            IDataStore store = XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.DatabaseAndSchema);
            XpoDefault.DataLayer = new SimpleDataLayer(dict, store);
            XpoDefault.Session = new Session();
        }
        static void Main(string[] args)
        {
            PrepareDb();
GetTrainData();


            Console.WriteLine("Hello World!");

            List<BetPoint> predictionData = new List<BetPoint>() { 
                DataManager.BetTemplates[0].Clone(), 
                DataManager.BetTemplates[6].Clone(),
                DataManager.BetTemplates[10].Clone() 
            };
            // RunComparingTrain(predictionData);            


            Console.WriteLine("----------- Model Manager First train ----------");
            Console.WriteLine();

            ModelManager modelManager = new ModelManager();
            modelManager.Train(dataManager.GenerateData(1).OfType<BetPoint>());

            IList<BetPointPrediction> resultData = modelManager.MakePredictions(predictionData);            
            foreach(BetPointPrediction res in resultData) {
                Console.WriteLine(string.Format("{0}", res.Gain));
            }
            Console.WriteLine("----------- Model Manager Second train ----------");
            Console.WriteLine();
            
            modelManager.Retrain(dataManager.GenerateData(2).OfType<BetPoint>());

            resultData = modelManager.MakePredictions(predictionData);            
            foreach(BetPointPrediction res in resultData) {
                Console.WriteLine(string.Format("{0}", res.Gain));
            }


            List<TransformedBetPoint> transformedPredictionData = new List<TransformedBetPoint>() { 
                DataManager.BetTemplates[0].CloneTransformed(), 
                DataManager.BetTemplates[6].CloneTransformed(),
                DataManager.BetTemplates[10].CloneTransformed() 
            };

            Console.WriteLine("----------- Model Manager First train ----------");
            Console.WriteLine();

            TransformedModelManager transformedModelManager = new TransformedModelManager();
            transformedModelManager.Train(dataManager.GenerateData(1).OfType<TransformedBetPoint>());

            IList<BetPointPrediction> transformedResultData = transformedModelManager.MakePredictions(transformedPredictionData);            
            foreach(BetPointPrediction res in transformedResultData) {
                Console.WriteLine(string.Format("{0}", res.Gain));
            }
            Console.WriteLine("----------- Model Manager Second train ----------");
            Console.WriteLine();
            
            transformedModelManager.Retrain(dataManager.GenerateData(2).OfType<TransformedBetPoint>());

            transformedResultData = transformedModelManager.MakePredictions(transformedPredictionData);            
            foreach(BetPointPrediction res in transformedResultData) {
                Console.WriteLine(string.Format("{0}", res.Gain));
            }

            Console.ReadLine();
        }
    }
}
