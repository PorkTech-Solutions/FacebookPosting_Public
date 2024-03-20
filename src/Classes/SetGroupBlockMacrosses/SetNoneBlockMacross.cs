using System.Runtime.Serialization;

namespace FacebookPosting.SetGroupBlockMacrosses;

[DataContract]
[KnownType(typeof(SetNoneBlockMacross))]
public class SetNoneBlockMacross : SetGroupBlockMacross
{
    public SetNoneBlockMacross(string data) 
        : base(data)
    {}

    public override void Set(IJoinGroupInputBlock joinGroupInputBlock)
    {
        Console.WriteLine($"INVOKED {Data}, SetNoneBlockMacross");
    }
}