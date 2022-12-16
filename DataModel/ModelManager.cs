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
    public class ModelManager {
        private DataManager dataManager;
        private IEstimator<ITransformer> pipeline;
        // private ITransformer dataPrepTransformer;
        private IEnumerable<IBetPoint> GetTrainData() {
            if(dataManager == null) {
                dataManager = new DataManager();
            }
            return dataManager.GenerateData();
        }

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

        public void Train(IEnumerable<BetPoint> rawTrainData) {
            MLContext mlContext = new MLContext(0);
            pipeline = mlContext.Transforms
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
                 .Append(mlContext.Transforms.NormalizeMinMax("Features"));
            IDataView trainData = mlContext.Data.LoadFromEnumerable(rawTrainData);
            ITransformer dataPrepTransformer = pipeline.Fit(trainData);
            IDataView transformedData = dataPrepTransformer.Transform(trainData);
            ITransformer trainedModel = mlContext.Regression.Trainers.OnlineGradientDescent().Fit(transformedData);
            mlContext.Model.Save(trainedModel, transformedData.Schema, "model.zip");
            SaveTransformer("Model", mlContext, transformedData.Schema, trainedModel);
            mlContext.Model.Save(dataPrepTransformer, trainData.Schema, "data_preparation_pipeline.zip");
            SaveTransformer("Pipeline", mlContext, trainData.Schema, dataPrepTransformer);
        }

        public void Retrain(IEnumerable<BetPoint> rawTrainData)
        {
            MLContext mlContext = new MLContext(0);
            IDataView trainData = mlContext.Data.LoadFromEnumerable(rawTrainData);
            DataViewSchema dataPrepPipelineSchema, modelSchema;
            ITransformer dataPrepTransformer = mlContext.Model.Load("data_preparation_pipeline.zip", out dataPrepPipelineSchema);
            // ITransformer trainedModel = mlContext.Model.Load("model.zip", out modelSchema);
            ITransformer trainedModel = LoadTransformer("Model", mlContext, out modelSchema);
            IDataView transformedData = dataPrepTransformer.Transform(trainData);
            LinearRegressionModelParameters originalModelParameters = ((ISingleFeaturePredictionTransformer<object>)trainedModel).Model as LinearRegressionModelParameters;
            ITransformer retrainedModel = mlContext.Regression.Trainers.OnlineGradientDescent().Fit(transformedData);
            mlContext.Model.Save(retrainedModel, transformedData.Schema, "model.zip");
            SaveTransformer("Model", mlContext, transformedData.Schema, retrainedModel);
        }

        public IList<BetPointPrediction> MakePredictions(IList<BetPoint> rawTestData) {
            MLContext mlContext = new MLContext(0);
            IList<BetPointPrediction> result = new List<BetPointPrediction>();
            DataViewSchema dataPrepPipelineSchema, modelSchema;

            ITransformer dataPrepTransformer = mlContext.Model.Load("data_preparation_pipeline.zip", out dataPrepPipelineSchema);
            // ITransformer trainedModel = mlContext.Model.Load("model.zip", out modelSchema);
            ITransformer trainedModel = LoadTransformer("Model", mlContext, out modelSchema);
            var predictionFunction = mlContext.Model.CreatePredictionEngine<TransformedBetPoint, BetPointPrediction>(trainedModel);
            IDataView testDataView = mlContext.Data.LoadFromEnumerable(rawTestData);
            IEnumerable<TransformedBetPoint> transformedData = mlContext.Data.CreateEnumerable<TransformedBetPoint>(dataPrepTransformer.Transform(testDataView), true);
            foreach(TransformedBetPoint betPoint in transformedData) {
                result.Add(predictionFunction.Predict(betPoint));
            }
            return result;
        }
    }
}