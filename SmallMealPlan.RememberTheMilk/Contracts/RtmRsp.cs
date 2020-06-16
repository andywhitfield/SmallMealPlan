namespace SmallMealPlan.RememberTheMilk.Contracts
{
    public abstract class RtmRsp
    {
        public string Stat { get; set; }
        public RtmErr Err { get; set; }

        public string InfoString => $"Stat={Stat}; Err={Err.Code}:{Err.Msg}";
    }
}