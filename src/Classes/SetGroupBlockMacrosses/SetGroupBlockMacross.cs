using System.Runtime.Serialization;

namespace FacebookPosting.SetGroupBlockMacrosses;

[DataContract]
[KnownType(typeof(SetGroupBlockMacross))]
public abstract class SetGroupBlockMacross : ISetGroupBlockMacross
{
    [DataMember]
    protected string _data;
    [IgnoreDataMember]
    public string Data => _data;
    public abstract void Set(IJoinGroupInputBlock joinGroupInputBlock);
    public SetGroupBlockMacross(string data)
    {
        _data = data;
    }
}