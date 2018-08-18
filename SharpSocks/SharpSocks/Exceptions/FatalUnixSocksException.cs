namespace SharpSocks.Exceptions
{
    public class FatalUnixSocksException : System.Exception
    {
        public FatalUnixSocksException(string message) : base(message) {}
    }
}