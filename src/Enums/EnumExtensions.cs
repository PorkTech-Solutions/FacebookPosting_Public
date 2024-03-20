namespace FacebookPosting.Enums;

public static class EnumExtensions
{
    public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        return GetFlags(value, Enum.GetValues<TEnum>());
    }

    public static IEnumerable<TEnum> GetIndividualFlags<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        return GetFlags(value, GetFlagValues<TEnum>().ToArray());
    }

    private static IEnumerable<TEnum> GetFlags<TEnum>(TEnum value, TEnum[] values)
        where TEnum : struct, Enum
    {
        ulong bits = Convert.ToUInt64(value);
        var results = new List<TEnum>();

        foreach (var enumValue in values.Reverse())
        {
            ulong mask = Convert.ToUInt64(enumValue);
            if ((bits & mask) == mask)
            {
                results.Add(enumValue);
                bits -= mask;
            }
        }

        return (bits == 0) ? results : Enumerable.Empty<TEnum>();
    }

    private static IEnumerable<TEnum> GetFlagValues<TEnum>()
        where TEnum : struct, Enum
    {
        ulong flag = 0x1;

        foreach (var value in Enum.GetValues<TEnum>())
        {
            ulong bits = Convert.ToUInt64(value);
            if (bits != 0)
            {
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }
    }

    public static TEnum CreateEnumFlag<TEnum>(this TEnum[] enumArray)
        where TEnum : struct, Enum
    {
        if (enumArray == null || enumArray.Length == 0)
        {
            throw new ArgumentException("Enum array cannot be null or empty.");
        }

        TEnum flagValue = enumArray[0];

        for (int i = 1; i < enumArray.Length; i++)
        {
            flagValue = (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt64(flagValue) | Convert.ToInt64(enumArray[i]));
        }

        return flagValue;
    }
}
