using System;
using System.Linq;
using System.Collections.Generic;

namespace MlNetExample
{

    public class DataManager
    {
        public static BetPoint CreateBetPoint(float koeff, float value, int minute, int maxDelta,
            SupplyingPlayer supplyingPlayer, float total, float delta, int period, float koefRatio,
            Gender gender, BettingFinal bettingFinal, BettingEvent betEvent, BetResult result)
        {
            return new BetPoint()
            {
                Koeff = koeff,
                Value = value,
                Minute = minute,
                MaxDelta = maxDelta,
                // SupplyingPlayer = supplyingPlayer,
                Total = total,
                Delta = delta,
                Period = period,
                KoefRatio = koefRatio,
                // Gender = gender,
                // BettingFinal = bettingFinal,
                // BetEvent = betEvent,
                BetResult = (float)(int)result
            };
        }
        public static List<BetPoint> BetTemplates = new List<BetPoint>() {
            CreateBetPoint(1.6F, 47.5F, 0, 3, SupplyingPlayer.Winning, 35, 1, 1, 1.4F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won),
            CreateBetPoint(1.6F, 47.5F, 0, 3, SupplyingPlayer.Winning, 35, 1, 1, 1.4F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Lose),
            CreateBetPoint(1.6F, 47.5F, 0, 3, SupplyingPlayer.Winning, 35, 1, 1, 1.4F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won),
            CreateBetPoint(1.5F, 47.5F, 0, 3, SupplyingPlayer.Winning, 35, 2, 1, 1.4F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won),
            CreateBetPoint(1.5F, 47.5F, 0, 3, SupplyingPlayer.Winning, 35, 2, 1, 1.4F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won),
            CreateBetPoint(1.9F, 47.5F, 0, 2, SupplyingPlayer.Winning, 40, 0, 1, 1.2F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Lose),
            CreateBetPoint(2.1F, 47.5F, 0, 1, SupplyingPlayer.Winning, 43, 1, 1, 1.2F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Lose),

            CreateBetPoint(1.4F, 47.5F, 0, 5, SupplyingPlayer.Winning, 30, 1, 1, 1.4F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won),
            CreateBetPoint(1.4F, 47.5F, 0, 1, SupplyingPlayer.Winning, 30, 1, 1, 1.4F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Lose),
            CreateBetPoint(1.2F, 47.5F, 0, 5, SupplyingPlayer.Winning, 30, 3, 1, 1.4F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won),
            CreateBetPoint(2F,   47.5F, 0, 6, SupplyingPlayer.Winning, 40, 0, 1, 1F,   Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won),
            CreateBetPoint(1.1F, 47.5F, 0, 2, SupplyingPlayer.Tie,     20, 0, 1, 1.2F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won),
            CreateBetPoint(1.4F, 47.5F, 0, 5, SupplyingPlayer.Winning, 30, 1, 1, 1.4F, Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won),
            CreateBetPoint(1.1F, 47.5F, 0, 1, SupplyingPlayer.Tie,     20, 0, 1, 1F,   Gender.Female, BettingFinal.TotalLower, BettingEvent.Period, BetResult.Won)
        };

        private List<IBetPoint> GenerateDataPart(int templatePart, bool transformed) {
            List<IBetPoint> data = new List<IBetPoint>();
            Random random = new Random();
            int templStart = templatePart > 0 ? (templatePart - 1) * 7 : 0;
            int templEnd = templatePart > 0 ? templatePart * 7 : BetTemplates.Count;
            Console.WriteLine(string.Format("startt: {0}, end: {1}", templStart, templEnd));
            int trainCount = 50000;            
            for (int i = 0; i < trainCount; i++)
            {
                int number = random.Next(templStart, templEnd);
                BetPoint template = BetTemplates[number];                
                if(transformed) { 
                    data.Add(template.CloneTransformed());
                }
                else {
                    data.Add(template.Clone());
                }
            }
            return data;
        }
        public List<IBetPoint> GenerateData(int templatePart = -1, bool transformed = false)
        {            
            if(templatePart > 0) {
                return GenerateDataPart(templatePart, transformed);
            }
            else {
                List<IBetPoint> data = new List<IBetPoint>();
                data.AddRange(GenerateDataPart(1, transformed));
                data.AddRange(GenerateDataPart(2, transformed));
                return data;
            }            
        }
    }
}