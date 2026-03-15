namespace STS2RitsuLib.Lifecycle.Patches
{
    internal static class LifecyclePatchTaskBridge
    {
        public static async Task After(Task originalTask, Action continuation)
        {
            ArgumentNullException.ThrowIfNull(originalTask);
            ArgumentNullException.ThrowIfNull(continuation);

            await originalTask;
            continuation();
        }

        public static async Task<T> After<T>(Task<T> originalTask, Action<T> continuation)
        {
            ArgumentNullException.ThrowIfNull(originalTask);
            ArgumentNullException.ThrowIfNull(continuation);

            var result = await originalTask;
            continuation(result);
            return result;
        }
    }
}
