using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace KeepAliveDotNetCore
{
    class Program
    {
        static void Main()
        {
            var requests = 0;
            var failures = 0;

            const string proto = "https";
            const string host = "speedtest.abilitybusiness.net";
            const int port = 443;

            var endPoints = Dns.GetHostEntry(host).AddressList.Select(entry => new IPEndPoint(entry, port));
            var uri = new Uri($"{proto}://{host}:{port}");

            var cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                    try
                    {

                        {
                            requests++;
                            var request = WebRequest.CreateHttp(uri);
                            request.Timeout = 1000;
                            request.KeepAlive = true;

                            var response = (HttpWebResponse)request.GetResponse();

                            var connections = IPGlobalProperties
                                .GetIPGlobalProperties()
                                .GetActiveTcpConnections()
                                .Where(c => endPoints.Contains(c.RemoteEndPoint))
                                .GroupBy(c => c.State)
                                .Select(g => new { State = g.Key, Count = g.Count(), Ports = g.Select(c => c.LocalEndPoint.Port.ToString()) })
                                .Select(c => $"{c.State}: {c.Count} ({c.Ports.Aggregate((l, r) => $"{l},{r}")})")
                                .Aggregate((l, r) => $"{l}; {r}");

                            Console.WriteLine($"Respone [{requests}] ContentLength [{response.ContentLength}] Connections [{connections}]");

                            response.Close();
                            Task.Delay(500).Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        failures++;
                        Console.WriteLine(ex);
                    }
            }, cts.Token);

            Console.ReadKey();
            cts.Cancel();
            Console.WriteLine($"Total Requests [{requests}] Failures [{failures}]");
            Console.ReadKey();
        }


    }
}