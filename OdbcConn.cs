using System.Data.Odbc;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;

namespace OdbcConnector
{
    public class OdbcConn
    {
        public string GetFullConnectionString(string Driver, string Database, string User, string PWD)
        {
            string ConnectionParams = "DRIVER={" + Driver + "};" +
                                      "SERVER=localhost;" +
                                      $"DATABASE={Database};";

            if (User != null)
            {
                ConnectionParams += $"UID={User};";
            }

            if (PWD != null)
            {
                ConnectionParams += $"PASSWORD={PWD};";
            }
            ConnectionParams += "OPTION=3";
            return ConnectionParams;
        }

        public string GetDataInJsonString(string DSN, string QueryString)
        {
            OdbcConnection DbConnection = new OdbcConnection("DSN=" + DSN);

            try
            {
                DbConnection.Open();
                WinLogger.cWinLogger.Logger.LogInformation($"Подключение к базе данных через DSN {DSN} выполнено.");
            }
            catch (OdbcException ex)
            {
                //Console.WriteLine($"Подключение к базе данных через DSN {DSN} не удалось:\n{ex.Message}");
                WinLogger.cWinLogger.Logger.LogError($"Подключение к базе данных через DSN {DSN} не удалось:\n{ex.Message}");
                return $"Подключение к базе данных через DSN {DSN} не удалось:\n{ex.Message}";
            }

            OdbcCommand DbCommand = DbConnection.CreateCommand();
            DbCommand.CommandText = QueryString;

            try
            {
                OdbcDataReader DbReader = DbCommand.ExecuteReader();
                WinLogger.cWinLogger.Logger.LogInformation($"Данные по запросу {QueryString} получены");

                int fieldCount = DbReader.FieldCount;
                string[] colNames = new string[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    colNames[i] = DbReader.GetName(i);
                }


                int j = 0;
                int rowCount = DbReader.RecordsAffected;
                string[,] colData = new string[fieldCount, rowCount];

                while (DbReader.Read())
                {
                    for (int i = 0; i < fieldCount; i++)
                    {
                        colData[i, j] += DbReader.GetString(i);
                    }

                    if (j < rowCount)
                    {
                        j = j + 1;
                    }
                }

                string jsonString = "{";
                for (int i = 0; i < fieldCount; i++)
                {
                    jsonString += '"' + colNames[i] + '"' + ": [";
                    for (j = 0; j < rowCount; j++)
                    {
                        jsonString += '"' + colData[i, j] + '"';

                        if ((j + 1) < rowCount)
                        {
                            jsonString += ", ";
                        }
                    }
                    jsonString += "]";

                    if ((i + 1) < fieldCount)
                    {
                        jsonString += ", ";
                    }
                }
                jsonString += "}";

                DbReader.Close();
                DbCommand.Dispose();
                DbConnection.Close();

                return jsonString;
            }
            catch (OdbcException ex)
            {
                //Console.WriteLine($"Данные не получены или не удалось объединить полученные данные в JSON:\n{ex.Message}");
                WinLogger.cWinLogger.Logger.LogError($"Данные не получены или не удалось объединить полученные данные в JSON:\n{ex.Message}");
                return $"Данные не получены или не удалось объединить полученные данные в JSON:\n{ex.Message}";
            }
        }

        public JObject GetDataInJsonObject(string DSN, string QueryString)
        {
            string jsonString = GetDataInJsonString(DSN, QueryString);
            //Console.WriteLine(jsonString.Count());
            JObject jsonObject = JObject.Parse(jsonString);

            return jsonObject;
        }

        public static byte[] Insert(ref byte[] array, byte value, int index)
        {
            byte[] newArray = new byte[array.Length + 1];
            newArray[index] = value;

            for (int i = 0; i < index; i++)
                newArray[i] = array[i];

            for (int i = index; i < array.Length; i++)
                newArray[i + 1] = array[i];

            array = newArray;
            return array;
        }
    }
}
