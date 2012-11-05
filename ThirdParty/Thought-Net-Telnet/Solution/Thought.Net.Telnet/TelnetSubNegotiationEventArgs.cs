using System;

namespace Thought.Net.Telnet
{
    /// <summary>
    /// event arguments for telnet sub negotation communication
    /// </summary>
	public class TelnetSubNegotiationEventArgs : EventArgs
	{
		
		private TelnetOption option;
		private byte[] data;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelnetSubNegotiationEventArgs" /> class.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="data">The data.</param>
		public TelnetSubNegotiationEventArgs (TelnetOption option, byte[] data)
		{
			this.option = option;
			this.data = data;
		}

		/// <summary>
		///     The <see cref="TelnetOption"/> related to the event..
		/// </summary>
		/// <seealso cref="TelnetOption"/>
		public TelnetOption Option
		{
			get
			{
				return this.option;
			}
		}

		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <value>
		/// The data.
		/// </value>
		public byte[] Data
		{
			get
			{
				return this.data;
			}
		}
	}
}

