using System;
using Microsoft.SPOT;

namespace RCDriveController
{
    public struct Vector
    {
        public static Vector zero = new Vector(0.0F, 0.0F);

        public float x { get; private set; }
        public float y { get; private set; }

        public Vector(float x, float y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Vector lhs, Vector rhs)
        {
            return lhs.x == rhs.x && rhs.y == lhs.y;
        }

        public static bool operator !=(Vector lhs, Vector rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "{" + this.x + ", " + this.y + "}";
        }
    }
}