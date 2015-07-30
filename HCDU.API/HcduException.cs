using System;

namespace HCDU.API
{
    public class HcduException : Exception
    {
        public HcduException(string message) : base(message)
        {
        }
    }
}