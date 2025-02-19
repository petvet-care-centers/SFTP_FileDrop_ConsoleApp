using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using SFTP_FileDrop_WorkerSerivce.Classes;
using SFTP_FileDrop2.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SFTP_FileDrop2.Classes
{
    class CsvFileGeneration
    {
        private readonly IConfiguration _configuration;
        private readonly Email _email;

        public CsvFileGeneration(IConfiguration configuration)
        {
            _configuration = configuration;
            _email = new Email(configuration);

        }

        public void Run()
        {
            Log.Info("Program has started.");
            Console.WriteLine(Directory.GetCurrentDirectory());
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            string? connectionString = _configuration.GetConnectionString("connstr");
            string? query = configuration["SqlSettings:DailyRevQuery"];
            string? sftpUsername = configuration["appsettings:SftpUsername"];
            string? sftpPassword = configuration["appsettings:SftpPassword"];
            string? sftpHost = configuration["appsettings:SftpHost"];
            string? sftpFolderPath = configuration["appsettings:SftpFolderPath"];
            string? processName = configuration["appsettings:ProcessName"];

            string formattedDate = DateTime.Now.ToString("MMddyyyy");
            string localFileName = $"SalesTransaction {formattedDate}.csv";
            string? sftpFilePath = sftpFolderPath + "/" + localFileName;

            DataTable dataTable = GenerateDataTable(connectionString, query);
            ConvertDataTableToCsv(localFileName, dataTable);
            UploadToSftp(localFileName, sftpHost, sftpUsername, sftpPassword, sftpFilePath);
            DeleteCsvFile(localFileName);
            _email.SendEmail(processName, $"{processName} Completed", $"{processName} has completed. File name: {localFileName}");

        }

        static void DeleteCsvFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    Log.Info($"Deleteing {fileName}.csv from local folder.");

                    File.Delete(fileName);
                }

            }
            catch (Exception ex)
            {
                Log.Exception("DeleteCsvFile", ex);
            }

        }
        static DataTable GenerateDataTable(string connString, string sqlQuery)
        {
            var dataTable = new DataTable();
            try
            {

                Log.Info("Creating DataTable.");

                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (var command = new SqlCommand(sqlQuery, conn))
                    using (var reader = command.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }

                }

                Log.Info("DataTable generated. Moving on to create csv file.");

            }
            catch (Exception ex)
            {
                Log.Exception("GenerateDataTable", ex);
            }

            return dataTable;

        }

        static void ConvertDataTableToCsv(string localFilePath, DataTable dt)
        {
            try
            {
                var csvContent = new StringBuilder();
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    csvContent.Append(col.ColumnName + ",");
                }
                csvContent.Length--;
                csvContent.AppendLine();

                foreach (DataRow row in dt.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        csvContent.Append(item.ToString() + ",");
                    }

                    csvContent.Length--;
                    csvContent.AppendLine();
                }

                File.WriteAllText(localFilePath, csvContent.ToString(), new UTF8Encoding(false));

                Log.Info($"{localFilePath}.csv created. Moving on to upload file to SFTP folder.");
            }
            catch (Exception ex)
            {
                Log.Exception("ConvertDataTableToCsv", ex);
            }

        }

        static void UploadToSftp(string localFilePath, string host, string username, string password, string remotePath)
        {
            try
            {
                using (SftpClient client = new(host, username, password))
                {
                    client.Connect();

                    Log.Info("Connected to SFTP server...");

                    using (FileStream fs = new(localFilePath, FileMode.Open))
                    {
                        client.UploadFile(fs, remotePath);
                    }

                    Log.Info("File uploaded successfully!");

                    client.Disconnect();
                }
            }

            catch (Exception ex)
            {
                Log.Exception("UploadToSftp", ex);

            }
        }


    }
}
