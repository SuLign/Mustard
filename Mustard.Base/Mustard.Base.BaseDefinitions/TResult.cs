namespace Mustard.Base.BaseDefinitions;

public class TResult
{
    public bool ResultState { get; set; }
    public object ResultData { get; set; }
    public string ResultMessage { get; set; }

    public TResult()
    {

    }

    public TResult(bool resultState) : this()
    {
        ResultState = resultState;
    }

    public TResult(string resultMessage) : this(false)
    {
        ResultMessage = resultMessage;
    }

    public TResult(bool resultState, object resultData) : this(resultState)
    {
        ResultData = resultData;
    }

    public TResult(bool resultState, object resultData, string resultMessage) : this(resultState)
    {
        ResultData = resultData;
        ResultMessage = resultMessage;
    }

    public bool TryGetContent<T>(out T outVal)
    {
        if (ResultData is T val)
        {
            outVal = (T)val;
            return true;
        }
        outVal = default;
        return false;
    }

    #region implicit methods
    public static implicit operator string(TResult result) => result.ResultMessage;

    public static implicit operator bool(TResult result) => result.ResultState;

    public static implicit operator TResult(bool result) => new TResult() { ResultState = result };

    public static implicit operator TResult(string result) => new TResult() { ResultMessage = result, ResultState = false };
    #endregion

    public override string ToString()
    {
        return $"state: {(ResultState ? "TRUE" : "FALSE")};\tvalue:{ResultData?.ToString()},\tmsg: {ResultMessage}";
    }
}

