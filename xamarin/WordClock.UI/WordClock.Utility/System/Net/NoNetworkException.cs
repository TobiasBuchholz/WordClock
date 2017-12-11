using System;
namespace System.Net
{
    public class NoNetworkException : Exception
    {
        public NoNetworkException()
            : base("No network connection")
        {
        }
    }
}
