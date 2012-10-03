namespace ArtheaEngine
{
    using System.Collections.Generic;
    using System.Timers;


    /// <summary>
    /// Tickable interface.
    /// </summary>
    public interface ITickable
    {
        #region Properties

        /// <summary>
        /// Gets the tick interval.
        /// </summary>
        /// <value>
        /// The tick interval.
        /// </value>
        int TickInterval
        {
            get;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Tick this instance.
        /// </summary>
        void Tick();

        #endregion Methods
    }

    /// <summary>
    /// Update manager.
    /// </summary>
    public static class UpdateManager
    {
        #region Fields

        static readonly Dictionary<ITickable, Timer> _timers = new Dictionary<ITickable, Timer>();

        #endregion Fields

        #region Methods

        /// <summary>
        /// Add the specified ticker.
        /// </summary>
        /// <param name='ticker'>
        /// Ticker.
        /// </param>
        public static void Add(ITickable ticker)
        {
            var timer = new Timer { Interval = ticker.TickInterval, Enabled = true };

            timer.Elapsed += delegate(object t, ElapsedEventArgs e)
            {
                ticker.Tick();
            };

            _timers.Add(ticker, timer);

            timer.Start();
        }

        /// <summary>
        /// Remove the specified ticker.
        /// </summary>
        /// <param name='ticker'>
        /// Ticker.
        /// </param>
        public static void Remove(ITickable ticker)
        {
            if (!_timers.ContainsKey(ticker)) return;

            _timers[ticker].Stop();

            _timers.Remove(ticker);
        }

        #endregion Methods
    }
}