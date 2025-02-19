using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFTP_FileDrop2.Classes;
using System;




using System.IO;



class Program
{
    static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var csvFileGeneration = host.Services.GetRequiredService<CsvFileGeneration>();
        csvFileGeneration.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<CsvFileGeneration>();
            });


}