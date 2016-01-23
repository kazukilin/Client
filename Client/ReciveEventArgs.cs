using System;

public class ReciveEventArgs : EventArgs
{
    public string message;
    public ReciveEventArgs(string message)
    {
        this.message = message;
    }
}
