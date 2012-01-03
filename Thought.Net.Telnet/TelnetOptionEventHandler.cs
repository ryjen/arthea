namespace Thought.Net.Telnet
{
    #region Delegates

    /// <summary>
    ///     Defines the signature of an event handler for the
    ///     option-related events of the <see cref="TelnetClient"/> class.
    /// </summary>
    public delegate void TelnetOptionEventHandler(
        object sender,
        TelnetOptionEventArgs e);

    #endregion Delegates
}