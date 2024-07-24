namespace NetCraft.Models;

public class TextureAltas
{
    public List<Texture> Textures { get; protected set; } = new();
    public static TextureAltas NullTextures { get; } = new("null");
    public string Id { get; protected init; }

    public TextureAltas(string id)
    {
        Id = id;
        Textures.Add(Texture.LoadSpecularFromId(id));
        Textures.Add(Texture.LoadDiffuseFromId(id));
    }
}
