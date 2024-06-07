namespace LocalManipulator.Helpers;

public class RunCodeResult
{
    public RunCodeResult(string output, string err)
    {
        Output = output;
        Err = err;
    }

    public string Output { get; set; }
    public string Err { get; set; }
}