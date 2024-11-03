using System.Net;
using System.Text;

public static class HttpExtensions
{
    public static void WriteString(this HttpListenerResponse response, string content, string contentType)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        response.OutputStream.Write(bytes, 0, bytes.Length);
        response.ContentType = contentType;
    }
}