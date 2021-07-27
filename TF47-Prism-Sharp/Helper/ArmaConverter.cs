using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TF47_Prism_Sharp.Helper
{
    public static class ArmaConverter
    {
        public static string ToArmaArray<T>(this List<T> data)
        {
            var stringBuilder = new StringBuilder();
            if (!data.Any()) return "[]";

            stringBuilder.Append('[');
            
            for (var i = 0; i < data.Count-1; i++)
            {
                if (data[i].GetType().IsGenericType)
                    stringBuilder.Append((data[i] as List<object>).ToArmaArray());
                else 
                    stringBuilder.Append($"{data[i]},");
            }

            if (data[^1].GetType().IsGenericType)
                stringBuilder.Append((data[^1] as List<object>).ToArmaArray());
            else 
                stringBuilder.Append($"{data[^1]}");
            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }
    }
}