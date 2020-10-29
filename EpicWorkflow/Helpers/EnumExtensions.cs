using System;
using System.ComponentModel;

namespace EpicWorkflow.Helpers
{
    public static class EnumExtensions
    {
        private static string GetEnumDescription(Enum enumElement)
        {
            var type = enumElement.GetType();

            var memInfo = type.GetMember(enumElement.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute) attrs[0]).Description;
            }

            return enumElement.ToString();
        }

        public static string ToDescription<TEnum>(this TEnum EnumValue) where TEnum : struct
        {
            return GetEnumDescription((Enum) (object) EnumValue);
        }
    }
}