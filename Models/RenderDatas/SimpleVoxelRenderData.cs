using NetCraft.Models.Enums;
using NetCraft.Utils;
using OpenTK.Mathematics;

namespace NetCraft.Models.RenderDatas;

public sealed class SimpleVoxelRenderData : RenderData
{
    public override RenderType Type => RenderType.SimpleVoxel;
    private EntityModel model = ModelManager.Instance.Models["simpleVoxel"];
    public float[] Vertices => model.Vertices;
    public int[] Indices => model.Indices;
    public Shader Shader => Shader.GetShaderFromId("simpleVoxel");

    public required TextureAltas TextureAltas { get; init; }
    public required Vector3i Location { get; init; }
}
