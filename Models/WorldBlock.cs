using NetCraft.Models.Enums;
using NetCraft.Models.Lights;
using NetCraft.Models.RenderDatas;
using NetCraft.Models.Structs;
using NetCraft.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace NetCraft.Models;

public class WorldBlock
{
    public WorldBlock(string shader)
    {
        Shader = Shader.GetShaderFromId(shader ?? "simpleVoxel");
        model = ModelManager.Instance.Models["simpleVoxel"];
        var debug = new DebugCapability(this);
        Capabilities.Add(debug);
    }

    public IList<Capability> Capabilities = new List<Capability>();

    private EntityModel model;

    public required TextureAltas Textures { get; init; }

    public required Vector3i Location { get; init; }
    public Vector2i ChunkLocation => (Location.X % Chunk.SizeX, Location.Z % Chunk.SizeZ);
    public Vector3i LocalLocation =>
        (
            Location.X - ChunkLocation.X * Chunk.SizeX,
            Location.Y,
            Location.Z - ChunkLocation.Y * Chunk.SizeZ
        );

    public Shader Shader { get; init; }
    public PointLight? PointLight { get; init; }

    public BlockFaceVisible FaceVisible { get; set; } = (BlockFaceVisible)0b111111;

    public List<float> GetVertices(List<float> collection)
    {
        if (FaceVisible.HasFlag(BlockFaceVisible.Top))
            collection.AddRange(model.Vertices.Take(new Range(0, 6)));
        if (FaceVisible.HasFlag(BlockFaceVisible.Bottom))
            collection.AddRange(model.Vertices.Take(new Range(6, 12)));
        if (FaceVisible.HasFlag(BlockFaceVisible.XyFront))
            collection.AddRange(model.Vertices.Take(new Range(12, 18)));
        if (FaceVisible.HasFlag(BlockFaceVisible.XyBack))
            collection.AddRange(model.Vertices.Take(new Range(18, 24)));
        if (FaceVisible.HasFlag(BlockFaceVisible.ZyFront))
            collection.AddRange(model.Vertices.Take(new Range(24, 30)));
        if (FaceVisible.HasFlag(BlockFaceVisible.ZyBack))
            collection.AddRange(model.Vertices.Take(new Range(30, 36)));
        return collection;
    }

    public string GetFaceCullingString()
    {
        System.Text.StringBuilder builder = new();

        if (FaceVisible.HasFlag(BlockFaceVisible.Top))
            builder.Append("Top ");
        if (FaceVisible.HasFlag(BlockFaceVisible.Bottom))
            builder.Append("Bottom ");
        if (FaceVisible.HasFlag(BlockFaceVisible.XyFront))
            builder.Append("XyFront ");
        if (FaceVisible.HasFlag(BlockFaceVisible.XyBack))
            builder.Append("XyBack ");
        if (FaceVisible.HasFlag(BlockFaceVisible.ZyFront))
            builder.Append("ZyFront ");
        if (FaceVisible.HasFlag(BlockFaceVisible.ZyBack))
            builder.Append("ZyBack ");
        return builder.ToString().TrimEnd();
    }

    protected static int CountTrueFlags<T>(T flags)
        where T : Enum
    {
        int count = 0;
        int flagValue = Convert.ToInt32(flags);

        while (flagValue != 0)
        {
            flagValue &= (flagValue - 1);
            count++;
        }

        return count;
    }

    public void Render(RenderContext context)
    {
        var voxel = new SimpleVoxelRenderData()
        {
            Location = this.Location,
            TextureAltas = this.Textures
        };

        context.RenderDatas.Add(voxel);
        if (this.PointLight is null)
        {
            return;
        }

        var light = new PointLightRenderData()
        {
            Intensity = this.PointLight.Intensity,
            Constant = this.PointLight.Constant,
            Linear = this.PointLight.Linear,
            Quadratic = this.PointLight.Quadratic,

            Position = this.PointLight.Position,
            Color = this.PointLight.Color,
            Ambient = this.PointLight.Ambient,
            Specular = this.PointLight.Specular,
        };
        context.RenderDatas.Add(light);
    }
}
