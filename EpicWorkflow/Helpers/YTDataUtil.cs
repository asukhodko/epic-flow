using System;
using Newtonsoft.Json.Linq;

namespace EpicWorkflow.Helpers
{
    public static class YTDataUtil
    {
        public static DateTime ParseYTDate(this long milliseconds)
        {
            var offset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return offset.DateTime;
        }

        public static DateTime ParseYTDate(this string milliseconds)
        {
            return ParseYTDate(long.Parse(milliseconds));
        }

        public static TEnum ParseEnum<TEnum>(dynamic fieldValue) where TEnum : struct
        {
            return EnumUtil.Parse<TEnum>(((JObject) fieldValue).GetValue("name").ToString().Replace(" ", ""));
        }
    }
}