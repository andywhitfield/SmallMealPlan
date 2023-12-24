namespace SmallMealPlan.RememberTheMilk.Contracts;

public class RtmJsonResponse<T> where T : RtmRsp
{
    public T? Rsp { get; set; }

    public bool IsSuccess => Rsp?.Stat == "ok" && Rsp?.Err == null;
}