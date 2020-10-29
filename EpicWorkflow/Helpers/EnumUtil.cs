using System;
using System.Collections.Generic;
using System.Linq;

namespace EpicWorkflow.Helpers
{
    public static class EnumUtil
    {
        public static List<TEnum> ToList<TEnum>() where TEnum : struct
        {
            return ((TEnum[]) Enum.GetValues(typeof(TEnum))).ToList();
        }

        public static TEnum Parse<TEnum>(string value) where TEnum : struct
        {
            return (TEnum) Enum.Parse(typeof(TEnum), value);
        }
    }
}