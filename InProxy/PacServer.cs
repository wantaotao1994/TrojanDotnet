using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Winter.InProxy
{
  public  class PacServer
    {
        static bool running = true;

        // This example requires the System and System.Net namespaces.
        public async System.Threading.Tasks.Task SimpleListenerExampleAsync(string[] prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();
            Byte[] buffer = await File.ReadAllBytesAsync("Pac/proxy.pac");
            while (running)
            {
                HttpListenerContext context = listener.GetContext();

                HttpListenerRequest request = context.Request;

                HttpListenerResponse response = context.Response;
                response.ContentType = "application/x-ns-proxy-autoconfig";


                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;

                System.IO.Stream output = response.OutputStream;

                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }
            // Note: The GetContext method blocks while waiting for a request.

            listener.Stop();

        }
    }
}
