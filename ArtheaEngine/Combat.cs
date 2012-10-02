using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using ArtheaEngine.Model;

namespace ArtheaEngine
{
	/// <summary>
	/// Combat.
	/// </summary>
    public class Combat : ITickable
    {
        #region Fields

        private Character _thisChar;

        #endregion Fields

        #region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Combat"/> class.
		/// </summary>
		/// <param name='ch'>
		/// Ch.
		/// </param>
		/// <param name='victims'>
		/// Victims.
		/// </param>
        public Combat(Character ch, params Character[] victims)
        {
            _thisChar = ch;
            Victims = new HashSet<Character>();
            foreach (var victim in victims)
                Victims.Add(victim);
        }

        #endregion Constructors

        #region Properties

		/// <summary>
		/// Gets or sets the speed.
		/// </summary>
		/// <value>
		/// The speed.
		/// </value>
        public int Speed
        {
            get;
            set;
        }

		/// <summary>
		/// Gets the tick interval.
		/// </summary>
		/// <value>
		/// The tick interval.
		/// </value>
        public int TickInterval
        {
            get { return Speed; }
        }

		/// <summary>
		/// Gets the victims.
		/// </summary>
		/// <value>
		/// The victims.
		/// </value>
        public ISet<Character> Victims
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

		private static int[] GetAttacks(Dice d1, Dice d2)
		{
			Collection<int> values = new Collection<int>();
			var roll1 = d1.Roll();
			var roll2 = d2.Roll();
			
			foreach (var a in roll1)
			{
				foreach (var b in roll2)
				{
					if (a > b)
						values.Add(a);
				}
			}
			return values.ToArray();
		}

		private static int GetDamage(Dice d1, Dice d2)
		{
			int value = 0;

			foreach (var a in d1.Roll())
			{
				foreach (var b in d2.Roll())
				{
					if (a > b)
						value += a;
				}
			}
			return value;
		}

		/// <summary>
		/// Attack the specified victim.
		/// </summary>
		/// <param name='victim'>
		/// Victim.
		/// </param>
        public void Attack(Character victim)
        {
			int[] attacks = GetAttacks(_thisChar.Attack, victim.Defense);

            foreach (var bonus in attacks)
            {
                var damage = GetDamage(_thisChar.Attack, victim.Defense);

                damage += bonus;

                victim.Vitals.Life -= damage;

                DamMessage(victim, damage, false);

                if (victim.Vitals.Life <= 0)
                {
                    Die(victim);
                    return;
                }
            }

            CounterAttack(victim, attacks.Length);
        }

		/// <summary>
		/// Displaies the prompt.
		/// </summary>
        public void DisplayPrompt()
        {
            if (_thisChar is Player)
            {
                (_thisChar as Player).DisplayPrompt();
            }
        }

		/// <summary>
		/// Tick this instance.
		/// </summary>
        public void Tick()
        {
            foreach (var victim in Victims)
            {
                Attack(victim);

                _thisChar.WriteLine();
                victim.WriteLine();
            }

            DisplayPrompt();
        }

        private void CounterAttack(Character victim, int attacks)
        {
            int counterAttacks = victim.Attack.Count - attacks;

            for (var i = 0; i < counterAttacks; i++)
            {
                var damage = GetDamage(victim.Attack, _thisChar.Defense);
                _thisChar.Vitals.Life -= damage;

                DamMessage(victim, damage, true);

                if (_thisChar.Vitals.Life <= 0)
                {
                    Die(victim);
                    return;
                }
            }
        }

        private void DamMessage(Character victim, int damage, bool counter)
        {
            Character a, b;
            string vp, vs;
            if (counter)
            {
                a = victim; b = _thisChar;
                vs = "counter";
                vp = "counters";
            }
            else
            {
                a = _thisChar; b = victim;
                vs = "hit";
                vp = "hits";
            }

            if (damage > 0)
            {
                a.Act("You {0} {1} for {2} damage!", vs, b, damage);
                b.Act("{0} {1} you for {2} damage!", a, vp, damage);
                b.Room.Act(new[] { a, b }, "{0} {1} {2} for {3} damage!", a, vp, b, damage);
            }
            else
            {
                a.Act("You miss {1}.", b);
                b.Act("{0} misses you with thier {1}.", a, vs);
                b.Room.Act(new[] { a, b }, "{0} misses {1} with thier {2}.", a, b, vs);
            }
        }

        private void Die(Character victim)
        {
            victim.WriteLine("~RYou are DEAD!~x");
            _thisChar.WriteLine("{0} is ~RDEAD!~x", victim);
            victim.Room.Act(new[] { _thisChar, victim }, "{0} is dead", victim);
            UpdateManager.Remove(victim.Fighting);
            UpdateManager.Remove(_thisChar.Fighting);
            victim.Fighting = null;
            _thisChar.Fighting = null;
            if (victim is NonPlayer)
            {
                victim.Dispose();
            }
        }

        #endregion Methods
    }
}