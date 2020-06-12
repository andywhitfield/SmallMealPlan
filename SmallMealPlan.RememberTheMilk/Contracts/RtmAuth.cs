namespace SmallMealPlan.RememberTheMilk.Contracts
{
    public class RtmAuth
    {
        public string Token { get; set; }
        public string Perms { get; set; }
        public RtmUser User { get; set; }
    }
}