using System.Collections.Immutable;
using System.Reflection;

namespace FacebookPosting.SetGroupBlockMacrosses;

public static class SetGroupBlockMacrossParser
{
    private const char CROSSINPUT_SEPARATOR = ';';
    private const char INPUT_SEPARATOR = ':';
    private static readonly ImmutableDictionary<string, Type> stringToType;

    static SetGroupBlockMacrossParser()
    {
        var baseType = typeof(SetGroupBlockMacross);

        var typesInAssembly = Assembly.GetExecutingAssembly().GetTypes();

        var derivedTypes = typesInAssembly.Where(type => baseType.IsAssignableFrom(type) && type != baseType).ToArray();
        var tmpStringToTypeRange = new KeyValuePair<string, Type>[derivedTypes.Length];

        for (int i = 0; i < tmpStringToTypeRange.Length; i++)
        {
            tmpStringToTypeRange[i] = new KeyValuePair<string, Type>(derivedTypes[i].Name, derivedTypes[i]);
        }

        stringToType = ImmutableDictionary.CreateRange(tmpStringToTypeRange);
    }

    // Format: "typeName:dataString"
    public static SetGroupBlockMacross ParseMacross(string macrossInput)
    {
        if(string.IsNullOrWhiteSpace(macrossInput))
        {
            throw new ArgumentNullException("macrossInput");
        }

        string[] macrossSeparatedInput = macrossInput.Split(INPUT_SEPARATOR);

        if(macrossSeparatedInput.Length != 2)
        {
            throw new ArgumentException($"macrossInput({macrossInput}) does not consist of 2 elements separated by \'{INPUT_SEPARATOR}\'");
        }

        string macrossTypeName = macrossSeparatedInput.First().Trim();
        string macrossDataValue = macrossSeparatedInput.Last().Trim();

        Type neededSetType = stringToType[macrossTypeName];

            var cons = neededSetType.GetConstructors();
            foreach (var constructor in cons)
            {
                
                System.Console.WriteLine(constructor.IsPublic == true);
                System.Console.WriteLine(constructor.GetParameters().Length == 1);
                System.Console.WriteLine(constructor.GetParameters().First().ParameterType);
            }

        ConstructorInfo constructorInfo = neededSetType.GetConstructors()
            .First(constructor => 
                constructor.IsPublic == true 
                && constructor.GetParameters().Length == 1 
                && constructor.GetParameters().First().ParameterType == typeof(string)
            );

        var result = (SetGroupBlockMacross)constructorInfo.Invoke([macrossDataValue]);

        return result;
    }
    public static SetGroupBlockMacross[] ParseMacrosses(string macrossesInput)
    {
        if(string.IsNullOrWhiteSpace(macrossesInput))
        {
            return [];
        }

        string[] macrossesSeparatedInputs = macrossesInput.Split(CROSSINPUT_SEPARATOR)
            .Where(macrossInput => string.IsNullOrWhiteSpace(macrossInput) != true)
            .ToArray();

        var resultList = new List<SetGroupBlockMacross>();

        foreach (var yieldMacrossInput in macrossesSeparatedInputs)
        {
            if(string.IsNullOrWhiteSpace(yieldMacrossInput))
            {
                continue;
            }

            resultList.Add(ParseMacross(yieldMacrossInput));
        }

        return resultList.ToArray();
    }

    public static string SerializeMacross(SetGroupBlockMacross macross)
    {
        ArgumentNullException.ThrowIfNull(macross, nameof(macross));

        string macrossTypeName = macross.GetType().Name; 
        string macrossDataValue = macross.Data;

        return $"{macrossTypeName}:{macrossDataValue}";
    }

    public static string SerializeMacrosses(IEnumerable<SetGroupBlockMacross> macrosses)
    {
        ArgumentNullException.ThrowIfNull(macrosses, nameof(macrosses));

        string serializedMacrosses = string.Join(CROSSINPUT_SEPARATOR, 
            macrosses.Select(SerializeMacross));

        return serializedMacrosses;
    }
}