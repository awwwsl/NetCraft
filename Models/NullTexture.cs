namespace NetCraft.Models;

public class NullTexture : Texture
{
    public NullTexture()
        : base(0) { }

    public static NullTexture Instance { get; } = new();
}
