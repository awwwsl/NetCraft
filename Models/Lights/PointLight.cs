using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetCraft.Models.Lights;

public struct PointLight
{
    public required Vector3 Position { get; init; }

    public required float Constant { get; init; }
    public required float Linear { get; init; }
    public required float Quadratic { get; init; }

    public required Vector3 Ambient { get; init; }
    public required Vector3 Diffuse { get; init; }
    public required Vector3 Specular { get; init; }

    public PointLightRenderable GetAligned()
    {
        return new()
        {
            PositionX = this.Position.X,
            PositionY = this.Position.Y,
            PositionZ = this.Position.Z,
            Padding = 0f,

            AmbientR = this.Ambient.X,
            AmbientG = this.Ambient.Y,
            AmbientB = this.Ambient.Z,
            Constant = this.Constant,

            DiffuseR = this.Diffuse.X,
            DiffuseG = this.Diffuse.Y,
            DiffuseB = this.Diffuse.Z,
            Linear = this.Linear,

            SpecularR = this.Specular.X,
            SpecularG = this.Specular.Y,
            SpecularB = this.Specular.Z,
            Quadratic = this.Quadratic,
        };
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct PointLightRenderable
{
    public required float PositionX { get; init; }
    public required float PositionY { get; init; }
    public required float PositionZ { get; init; }
    public float Padding;

    public required float AmbientR { get; init; }
    public required float AmbientG { get; init; }
    public required float AmbientB { get; init; }
    public required float Constant { get; init; }

    public required float DiffuseR { get; init; }
    public required float DiffuseG { get; init; }
    public required float DiffuseB { get; init; }
    public required float Linear { get; init; }

    public required float SpecularR { get; init; }
    public required float SpecularG { get; init; }
    public required float SpecularB { get; init; }
    public required float Quadratic { get; init; }
}
