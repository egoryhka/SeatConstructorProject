using Newtonsoft.Json;
using System.IO;

namespace SeatConstructor.Models
{
    public static class Extensions
    {
        public static string ToJson(this object obj)
        {
            JsonSerializer js = JsonSerializer.CreateDefault();
            using (Stream stream = Stream.Null)
            {
                js.Serialize(new StreamWriter(stream), obj);
                return new StreamReader(stream).ReadToEnd();
            }
        }

        //public static object FromJson(this string json)
        //{
        //    return System.Text.Json.JsonSerializer.Deserialize<object>(json);
        //}

    }

}
