using System.Collections.Generic;

namespace Basement.BLFramework.Core.Util
{
    public static class SerializeUtil
    {
        public static IDictionary<string, object> SetArgs(this IDictionary<string, object> dict, params object[] args)
        {
            for (int i = 0; i < args.Length / 2; i++)
            {
                var value = args[i * 2 + 1];
                
                if (value != null)
                {
                    dict[(string) args[i * 2]] = value;
                }
            }

            return dict;
        }
        
        public static IList<Dictionary<string, object>> SetArgs(this IList<Dictionary<string, object>> dictArray, params object[] args)
        {
            var dict = new Dictionary<string, object>();
            dictArray.Add(dict);
            
            for (int i = 0; i < args.Length / 2; i++)
            {
                dict.Add((string) args[i * 2], args[i * 2 + 1]);
            }

            return dictArray;
        }

        public static IDictionary<string, object> Dict(object fromDictionary = null)
        {
            if (fromDictionary != null)
            {
                return (IDictionary<string, object>) fromDictionary;
            }

            return new Dictionary<string, object>();
        }
        
        public static IList<Dictionary<string, object>> DictArray(object fromDictArray = null)
        {
            if (fromDictArray != null)
            {
                return (IList<Dictionary<string, object>>) fromDictArray;
            }

            return new List<Dictionary<string, object>>();
        }


        public static string Json2String(object dict)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(dict);
        }
    }
}
