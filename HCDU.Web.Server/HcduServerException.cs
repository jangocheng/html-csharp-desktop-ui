using System;

namespace HCDU.Web.Server
{
    public class HcduServerException : Exception
    {
        public HcduServerException(string message) : base(message)
        {
        }
    }
}