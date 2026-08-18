using System;
using System.ComponentModel;

namespace Takenoko.Standard
{
    public static class EnumExtension
    {
        public static string Name(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attr?.Description ?? value.ToString();
        }
    }
}
