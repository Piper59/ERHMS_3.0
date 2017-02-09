﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ERHMS.Utility
{
    public static class EnumExtensions
    {
        public static TEnum Parse<TEnum>(string value)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }

        public static IEnumerable<TEnum> GetValues<TEnum>()
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        }

        private static string GetDescription(FieldInfo field)
        {
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? null : attribute.Description;
        }

        public static string ToDescription(Enum value)
        {
            return GetDescription(value.GetType().GetField(value.ToString()));
        }

        public static TEnum FromDescription<TEnum>(string description)
        {
            return (TEnum)typeof(TEnum).GetFields()
                .Where(field => field.FieldType == typeof(TEnum))
                .Single(field => GetDescription(field) == description)
                .GetValue(null);
        }
    }
}
