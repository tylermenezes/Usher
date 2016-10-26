namespace Usher.Platforms.Generic.Abilities
{
    public interface IRgb
    {
        int R {
            get;
        }
        int G {
            get;
        }
        int B {
            get;
        }

        void SetRgb(int r, int g, int b);
    }
}