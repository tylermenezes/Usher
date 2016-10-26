namespace Usher.Platforms.Generic.Abilities
{
    public interface IDimmable : ITogglable
    {
        decimal Dim {
            get;
            set;
        }
    }
}