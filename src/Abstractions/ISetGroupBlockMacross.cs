namespace FacebookPosting.Abstractions;

public interface ISetGroupBlockMacross
{
    string Data { get; }
    void Set(IJoinGroupInputBlock joinGroupInputBlock);
}