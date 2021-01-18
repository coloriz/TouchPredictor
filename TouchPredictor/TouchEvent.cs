namespace InsLab
{
    public struct TouchEvent<T> where T : struct
    {
        public enum Type { Start, End, Move };

        public T X { get; set; }
        public T Y { get; set; }
        public int Timestamp { get; set; }
        public Type EventType { get; set; }

        public TouchEvent(T x, T y, int timestamp, Type eventType=Type.Move)
        {
            X = x;
            Y = y;
            Timestamp = timestamp;
            EventType = eventType;
        }

        public override string ToString()
        {
            return $"TouchEvent(x={X}, y={Y}, timestamp={Timestamp}, type={EventType}";
        }
    }
}
