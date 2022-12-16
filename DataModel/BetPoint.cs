using Microsoft.ML.Data;

namespace MlNetExample
{
    public enum Gender
    {
        Male,
        Female,
        Mix,
        Any
    }
    public enum BetType
    {
        Winner,
        WinnerX,
        X,
        Fora,
        Total,
        TotalParity
    }
    public enum BettingFinal { First, Second, X, FirstX, SecondX, FirstSecond, Fora1, Fora2, TotalGreater, TotalLower, TotalEven, TotalOdd }
    public enum BetResult { InGame, Won, Lose }
    public enum BettingEvent { Match, Period }
    public enum SupplyingPlayer { Winning, Losing, Tie, None }

    public interface IBetPoint {}
    public class BetPoint : IBetPoint
    {
        public BetPoint Clone()
        {
            return new BetPoint()
            {
                Koeff = this.Koeff,
                Value = this.Value,
                Minute = this.Minute,
                MaxDelta = this.MaxDelta,
                // SupplyingPlayer = this.SupplyingPlayer,
                Total = this.Total,
                Delta = this.Delta,
                Period = this.Period,
                KoefRatio = this.KoefRatio,
                BetResult = (float)(int)this.BetResult
                // Gender = this.Gender,
                // BettingFinal = this.BettingFinal,
                // BetEvent = this.BetEvent
            };
        }

        public TransformedBetPoint CloneTransformed() {
            return new TransformedBetPoint() {
                Features = new float[] {
                    this.Koeff,
                    this.Value,
                    this.Minute,
                    this.MaxDelta,
                    this.Total,
                    this.Delta,
                    this.Period,
                    this.KoefRatio
                },
                Label = this.Gain
            };
        }

        [LoadColumn(0)]
        public float Koeff;

        [LoadColumn(1)]
        public float Value;

        [LoadColumn(2)]
        public float Minute;

        [LoadColumn(3)]
        public float MaxDelta;

        // [LoadColumn(4)]
        // public SupplyingPlayer SupplyingPlayer { get; set; }

        [LoadColumn(4)]
        public float Total;

        [LoadColumn(5)]
        public float Delta;

        [LoadColumn(6)]
        public float Period;

        [LoadColumn(7)]
        public float KoefRatio;

        // [LoadColumn(8)]
        // public Gender Gender { get; set; }

        // [LoadColumn(9)]
        // public string Country { get; set; }        
        // public BettingFinal BettingFinal { get; set; }

        // // [LoadColumn(11)]
        // public BettingEvent BetEvent { get; set; }

        // [System.ComponentModel.Browsable(false)]
        // public BetResult BetResult { get; set; }

        public float BetResult { get; set; }

        [LoadColumn(8)]
        public float Gain { get { return BetResult == 1 ? Koeff : 0; } }
    }

    public class BetPointPrediction
    {
        [ColumnName("Score")]
        public float Gain { get; set; }
    }
    public class TransformedBetPoint : IBetPoint
    {
        [VectorType(8)]
        public float[] Features { get; set; }
        public float Label { get; set; }
    }
}