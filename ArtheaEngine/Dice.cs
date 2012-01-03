namespace ArtheaEngine
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    public class Dice : Collection<Die>
    {
        #region Constructors

        public Dice(int count, int faces)
        {
            while (count-- > 0)
                Add(new Die(faces));
        }

        public Dice(params Die[] dice)
        {
            foreach (var d in dice)
                Add(d);
        }

        public Dice()
        {
        }

        #endregion Constructors

        #region Properties

        public int Bonus
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public int[] Roll()
        {
            int[] values = new int[Count];

            for (int index = 0; index < values.Length; index++)
            {
                values[index] = Items[index].Roll();
            }
            return values;
        }

        public int[] RollAgainst(Dice dice)
        {
            Collection<int> values = new Collection<int>();
            var roll1 = Roll();
            var roll2 = dice.Roll();

            foreach (var a in roll1)
            {
                foreach (var b in roll2)
                {
                    if (a >= b)
                        values.Add(a);
                }
            }
            return values.ToArray();
        }

        public int RollAgainstValue(Dice dice)
        {
            int value = 0;

            foreach (var a in Roll())
            {
                foreach (var b in dice.Roll())
                {
                    if (a >= b)
                        value += a;
                }
            }
            return value;
        }

        #endregion Methods
    }

    public class Die
    {
        #region Fields

        private static Random rand = new Random();

        #endregion Fields

        #region Constructors

        public Die(int faceCount)
        {
            if (faceCount <= 0)
            {
                throw new ArgumentOutOfRangeException("faceCount", "Dice must have one or more faces.");
            }

            FaceCount = faceCount;
        }

        #endregion Constructors

        #region Properties

        public int FaceCount
        {
            get; set;
        }

        public int Value
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public int Roll()
        {
            Value = rand.Next(1, FaceCount + 1);
            return Value;
        }

        #endregion Methods
    }
}