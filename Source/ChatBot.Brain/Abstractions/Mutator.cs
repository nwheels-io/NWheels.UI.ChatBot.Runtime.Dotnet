namespace ChatBot.Brain.Abstractions
{
    public struct Mutator<T>
    {
        public readonly T NewValue;

        public Mutator(T newValue)
        {
            NewValue = newValue;
        }
    }

    public static class MutatorExtensions
    {
        public static T MutatedOrOriginal<T>(Mutator<T>? mutator, T originalValue)
        {
            return mutator.HasValue
                ? mutator.Value.NewValue
                : originalValue;
        }
    }
}