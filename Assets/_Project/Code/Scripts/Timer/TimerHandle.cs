using System;

namespace _Project.Code.Scripts.Timer
{
    public readonly struct TimerHandle : IEquatable<TimerHandle>
    {
        public readonly int Id;

        internal TimerHandle(int id) => Id = id;

        public bool Equals(TimerHandle other) => Id == other.Id;
        public override bool Equals(object obj) => obj is TimerHandle other && Equals(other);
        public override int GetHashCode() => Id;
        public static bool operator ==(TimerHandle left, TimerHandle right) => left.Equals(right);
        public static bool operator !=(TimerHandle left, TimerHandle right) => !left.Equals(right);
    }
}
