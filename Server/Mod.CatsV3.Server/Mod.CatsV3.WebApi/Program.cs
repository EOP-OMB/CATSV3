using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mod.CatsV3.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("App supported protocols:   " + ServicePointManager.SecurityProtocol);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //     Host.CreateDefaultBuilder(args)
        //         .ConfigureWebHostDefaults(webBuilder =>
        //         {
        //             webBuilder.UseKestrel((options) =>
        //             {
        //                 // Do not add the Server HTTP header.
        //                 options.AddServerHeader = false;
        //                 options.ConfigureHttpsDefaults(s =>
        //                 {
        //                     s.SslProtocols = SslProtocols.Default | SslProtocols.Ssl3 | SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls | SslProtocols.Tls13;
        //                 });
        //             });
        //             webBuilder.UseStartup<Startup>();
        //         });
    }
}
