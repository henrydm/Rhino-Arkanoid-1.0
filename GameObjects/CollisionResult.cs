namespace RhinoArkanoid.GameObjects
{
    class CollisionResult
    {
        public enum ResultType { None, Wall, Pad, Block, Loose }
        public ResultType Result { get; }
        public Drawable Object { get; }

        public CollisionResult(ResultType result, Drawable obj)
        {
            Result = result;
            Object = obj;
        }
    }
}
