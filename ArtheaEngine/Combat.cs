namespace ArtheaEngine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    using ArtheaEngine.Model;

    public class Combat : ITickable
    {
        #region Fields

        private Character _thisChar;

        #endregion Fields

        #region Constructors

        public Combat(Character ch, params Character[] victims)
        {
            _thisChar = ch;
            Victims = new HashSet<Character>();
            foreach (var victim in victims)
                Victims.Add(victim);
        }

        #endregion Constructors

        #region Properties

        public int Speed
        {
            get;
            set;
        }

        public int TickInterval
        {
            get { return Speed; }
        }

        public ISet<Character> Victims
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        public void Attack(Character victim)
        {
            int[] attacks = _thisChar.Attack.RollAgainst(victim.Defense);

            foreach (var bonus in attacks)
            {
                var damage = _thisChar.Attack.RollAgainstValue(victim.Defense);

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

        public void DisplayPrompt()
        {
            if (_thisChar is Player)
            {
                (_thisChar as Player).DisplayPrompt();
            }
        }

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
                var damage = victim.Attack.RollAgainstValue(_thisChar.Defense);
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