using JeremyAnsel.Media.WavefrontObj;

public class EntityModel
{
    public float[] Vertices { get; }
    public int[] Indices { get; } = new int[1]; // FIXME:
    public int VerticesStartIndex;
    public int IndicesStartIndex;

    public EntityModel(string path, int verticesStartIndex, int indicesStartIndex)
    {
        ObjFile obj = ObjFile.FromFile(path);
        Vertices = obj
            .Vertices.Zip(obj.VertexNormals, (pos, norm) => new { pos, norm })
            .Zip(
                obj.TextureVertices,
                (tuple, tex) =>
                    new float[]
                    {
                        tuple.pos.Position.X,
                        tuple.pos.Position.Y,
                        tuple.pos.Position.Z,
                        tuple.norm.X,
                        tuple.norm.Y,
                        tuple.norm.Z,
                        tex.X,
                        tex.Y
                    }
            )
            .SelectMany(e => e)
            .ToArray();
        VerticesStartIndex = verticesStartIndex;
        IndicesStartIndex = indicesStartIndex;
    }
}
