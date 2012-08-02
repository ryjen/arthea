namespace ArtheaEngine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Timers;

    using ArtheaEngine.Model;

    public interface ITickable
    {
        #region Properties

        int TickInterval
        {
            get;
        }

        #endregion Properties

        #region Methods

        void Tick();

        #endregion Methods
    }

    public static class UpdateManager
    {
        #region Fields

        static readonly Dictionary<ITickable, Timer> _timers = new Dictionary<ITickable, Timer>();

        #endregion Fields

        #region Methods

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

        public static void Remove(ITickable ticker)
        {
            if (!_timers.ContainsKey(ticker)) return;

            _timers[ticker].Stop();

            _timers.Remove(ticker);
        }

        #endregion Methods
    }
}