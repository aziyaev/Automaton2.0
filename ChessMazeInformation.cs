using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    public class ChessMazeInformation
    {
        private readonly byte representation;

        private ChessMazeInformation()
        {
            representation = 0;
        }
        private ChessMazeInformation(byte representation)
        {
            this.representation = representation;
        }

        public static ChessMazeInformation L { get; }
        private static char LAsChar;
        public static ChessMazeInformation N { get; }
        public static ChessMazeInformation E { get; }
        public static ChessMazeInformation S { get; }
        public static ChessMazeInformation W { get; }

        private static readonly (ChessMazeInformation direction, char asChar)[] singles;

        static ChessMazeInformation()
        {
            L = new ChessMazeInformation(0b0000);
            LAsChar = 'l';

            N = new ChessMazeInformation(0b0001);
            E = new ChessMazeInformation(0b0010);
            S = new ChessMazeInformation(0b0100);
            W = new ChessMazeInformation(0b1000);

            singles = new (ChessMazeInformation, char)[] { (N, 'n'), (E, 'e'), (S, 's'), (W, 'w') };
        }

        public static ChessMazeInformation operator +(ChessMazeInformation info1, ChessMazeInformation info2)
        {
            var res = info1.representation | info2.representation;
            return new ChessMazeInformation((byte)res);
        }

        public static ChessMazeInformation operator +(ChessMazeInformation info1, string info2)
        {
            return info1 + FromString(info2);
        }

        public static implicit operator ChessMazeInformation(string str)
        {
            return FromString(str);
        }

        public static ChessMazeInformation FromString(string str)
        {
            var converted = str.ToLower().Select(c => FromChar(c));

            var result = new ChessMazeInformation();
            foreach (var info in converted)
                result += info;

            return result;
        }

        private static ChessMazeInformation FromChar(char c)
        {
            try
            {
                if (c == LAsChar)
                    return L;

                var res = singles.First(s => s.asChar == c);
                return res.direction;
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentOutOfRangeException(nameof(c), "Info can only be 'l', 'n', 'e', 's' or 'w'.");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!GetType().Equals(obj.GetType()))
                return false;

            return representation == ((ChessMazeInformation)obj).representation;
        }

        public override int GetHashCode()
        {
            return representation;
        }

        public override string ToString()
        {
            if (representation == 0)
                return LAsChar.ToString();

            var dirs = singles
                .Where(s => Contains(s.direction))
                .Select(s => s.asChar)
                .ToArray();

            return new string(dirs);
        }

        public bool Contains(ChessMazeInformation info)
        {
            return ((~representation) & info.representation) == 0;
        }

        public bool ContainsSingleDirection
        {
            get
            {
                if (representation == 0)
                    return true;
                return singles.Count(s => Contains(s.direction)) == 1;
            }
        }
    }
}