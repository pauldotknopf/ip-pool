namespace IpPool.Lib;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
        
    }

    public static void Throw(string message)
    {
        throw new BusinessException(message);
    }
}