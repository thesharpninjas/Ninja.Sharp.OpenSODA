// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Models;
using System.Data;
using System.Text;
using InvalidDataException = Ninja.Sharp.OpenSODA.Exceptions.InvalidDataException;

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Native.Extensions
{
    internal static class DataReaderExtensions
    {
        public static ICollection<Item<T>> ReadItems<T>(this IDataReader dataReader) where T : class, new()
        {
            List<Item<T>> list = [];

            while (dataReader.Read())
            {
                Item<T>? anItem = dataReader.ReadItem<T>();
                if (anItem != null)
                {
                    list.Add(anItem);
                }
            }

            return list;
        }

        public static Item<T>? ReadItem<T>(this IDataReader oracleReader) where T : class, new()
        {
            bool completeData = Enumerable.Range(0, oracleReader.FieldCount).Any(x => oracleReader.GetName(x) == "JSON_DOCUMENT");
            int ordinal = 0;
            if (completeData)
            {
                ordinal = oracleReader.GetOrdinal("JSON_DOCUMENT");
            }
            if (!oracleReader.IsDBNull(ordinal))
            {
                string jsonData = string.Empty;
                if (oracleReader.GetFieldType(ordinal) == typeof(string))
                {
                    jsonData = oracleReader.GetString(ordinal);
                }
                else if (oracleReader.GetFieldType(ordinal) == typeof(byte[]))
                {
                    jsonData = Encoding.Default.GetString((byte[])oracleReader.GetValue(ordinal));
                }

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    throw new InvalidDataException("Cannot retrieve json data");
                }

                Item<T> obj = new()
                {
                    Value = jsonData.Deserialize<T>()
                };
                if (completeData)
                {
                    obj.Id = oracleReader.GetString(oracleReader.GetOrdinal("ID"));
                    obj.Created = oracleReader.GetDateTime(oracleReader.GetOrdinal("CREATED_ON"));
                    obj.LastModified = oracleReader.GetDateTime(oracleReader.GetOrdinal("LAST_MODIFIED"));
                    obj.ETag = oracleReader.GetString(oracleReader.GetOrdinal("VERSION"));
                }
                return obj;
            }
            return null;
        }
    }
}
