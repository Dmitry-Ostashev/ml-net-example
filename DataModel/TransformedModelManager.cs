using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;

namespace MlNetExample {
    public class TransformedModelManager {
        private DataManager dataManager;
        private IEstimator<ITransformer> pipeline;

        private void SaveTransformer(string name, MLContext context, DataViewSchema schema, ITransformer transformer)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                ModelStorage storage = unitOfWork.FindObject(typeof(ModelStorage), new BinaryOperator("Name", name)) as ModelStorage;
                if (storage == null)
                {
                    storage = new ModelStorage(unitOfWork);
                    storage.Name = name;
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    context.Model.Save(transformer, schema, ms);
                    storage.Content = ms.ToArray();
                }
                storage.Save();
                unitOfWork.CommitChanges();
            }
        }

        private ITransformer LoadTransformer(string name, MLContext context, out DataViewSchema schema)
        {
            ITransformer result = null;
            schema = null;
            using (UnitOfWork unitOfWork = new UnitOfWork()){
                ModelStorage storage = unitOfWork.FindObject(typeof(ModelStorage), new BinaryOperator("Name", name)) as ModelStorage;
                if(storage != null) {
                    using(MemoryStream ms = new MemoryStream(storage.Content)) {
                        result = context.Model.Load(ms, out schema);
                    }                    
                }
            }
            return result;
        }

        public void Train(IEnumerable<TransformedBetPoint> rawTrainData) {
            MLContext mlContext = new MLContext(0);

            IDataView transformedData = mlContext.Data.LoadFromEnumerable(rawTrainData);
            ITransformer trainedModel = mlContext.Regression.Trainers.OnlineGradientDescent().Fit(transformedData);
            SaveTransformer("TransformedModel", mlContext, transformedData.Schema, trainedModel);
        }

        public void Retrain(IEnumerable<TransformedBetPoint> rawTrainData)
        {
            MLContext mlContext = new MLContext(0);
            IDataView trainData = mlContext.Data.LoadFromEnumerable(rawTrainData);
            DataViewSchema modelSchema;
            ITransformer trainedModel = LoadTransformer("TransformedModel", mlContext, out modelSchema);
            LinearRegressionModelParameters originalModelParameters = ((ISingleFeaturePredictionTransformer<object>)trainedModel).Model as LinearRegressionModelParameters;
            ITransformer retrainedModel = mlContext.Regression.Trainers.OnlineGradientDescent().Fit(trainData);            
            SaveTransformer("TransformedModel", mlContext, trainData.Schema, retrainedModel);
        }

        public IList<BetPointPrediction> MakePredictions(IList<TransformedBetPoint> rawTestData) {
            MLContext mlContext = new MLContext(0);
            IList<BetPointPrediction> result = new List<BetPointPrediction>();
            DataViewSchema modelSchema;
            ITransformer trainedModel = LoadTransformer("TransformedModel", mlContext, out modelSchema);
            var predictionFunction = mlContext.Model.CreatePredictionEngine<TransformedBetPoint, BetPointPrediction>(trainedModel);
            IDataView testDataView = mlContext.Data.LoadFromEnumerable(rawTestData);
            
            foreach(TransformedBetPoint betPoint in rawTestData) {
                result.Add(predictionFunction.Predict(betPoint));
            }
            return result;
        }
    }
}