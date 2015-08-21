using System.Text;

namespace HCDU.Web.Server
{
    public class WebSocketMessage
    {
        public byte OpCode { get; set; }
        public byte[] Content { get; set; }

        public string GetText()
        {
            return Encoding.UTF8.GetString(Content);
        }
    }
}