using NetCraft.Models.Enums;
using NetCraft.Models.Lights;
using NetCraft.Models.Structs;
using OpenTK.Mathematics;

namespace NetCraft.Models.RenderDatas;

public class PointLightRenderData : RenderData
{
    public override RenderType Type => RenderType.PointLight;
    public required Vector3 Position { get; init; }

    public required float Constant { get; init; }
    public required float Linear { get; init; }
    public required float Quadratic { get; init; }
    public required float Intensity { get; init; }

    public required Vector3 Ambient { get; init; }
    public required Vector3 Color { get; init; }
    public required Vector3 Specular { get; init; }

    public PointLightRenderable GetAligned()
    {
        return new()
        {
            PositionX = this.Position.X,
            PositionY = this.Position.Y,
            PositionZ = this.Position.Z,
            Intensity = this.Intensity,

            AmbientR = this.Ambient.X,
            AmbientG = this.Ambient.Y,
            AmbientB = this.Ambient.Z,
            Constant = this.Constant,

            ColorR = this.Color.X,
            ColorG = this.Color.Y,
            ColorB = this.Color.Z,
            Linear = this.Linear,

            SpecularR = this.Specular.X,
            SpecularG = this.Specular.Y,
            SpecularB = this.Specular.Z,
            Quadratic = this.Quadratic,
        };
    }
}
