using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace HCDU.API
{
    public class MethodContentProvider<TResult> : IContentProvider
    {
        private readonly Func<TResult> method;

        public MethodContentProvider(Func<TResult> method)
        {
            this.method = method;
        }

        public bool IsStatic
        {
            get { return false; }
        }

        public HttpResponse GetContent()
        {
            try
            {
                TResult result = method();

                //todo: handle null
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TResult));
                MemoryStream mem = new MemoryStream();
                ser.WriteObject(mem, result);

                return HttpResponse.Ok(MimeTypes.Json, mem.ToArray());
            }
            catch (Exception e)
            {
                return CreateErrorResponse(e);
            }
        }

        private HttpResponse CreateErrorResponse(Exception ex)
        {
            string content = string.Format("{0}: {1}\n---------------------\n{2}", ex.GetType().FullName, ex.Message, ex.StackTrace);
            return HttpResponse.InternalServerError(MimeTypes.PlainText, Encoding.UTF8.GetBytes(content));
        }
    }
}