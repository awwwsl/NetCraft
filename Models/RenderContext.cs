using NetCraft.Models.RenderDatas;

namespace NetCraft.Models;

public class RenderContext
{
    public List<RenderData> RenderDatas { get; } = new();

    public IEnumerable<PointLightRenderData> GetPointLightRenderDatas() =>
        RenderDatas
            .Where(e => e.Type == Enums.RenderType.PointLight)
            .Select(e => (PointLightRenderData)e);

    public IEnumerable<SimpleVoxelRenderData> GetSimpleVoxelRenderDatas() =>
        RenderDatas
            .Where(e => e.Type == Enums.RenderType.SimpleVoxel)
            .Select(e => (SimpleVoxelRenderData)e);
}
