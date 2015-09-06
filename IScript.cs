namespace LiveSplit
{
    public interface IScript
    {
        dynamic this[string name] { get; set; }
        dynamic Run();
    }
}
