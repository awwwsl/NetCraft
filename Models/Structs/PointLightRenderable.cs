using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace NetCraft.Models.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct PointLightRenderable
{
    public required float PositionX { get; init; }
    public required float PositionY { get; init; }
    public required float PositionZ { get; init; }
    public required float Intensity { get; init; }

    public required float AmbientR { get; init; }
    public required float AmbientG { get; init; }
    public required float AmbientB { get; init; }
    public required float Constant { get; init; }

    public required float ColorR { get; init; }
    public required float ColorG { get; init; }
    public required float ColorB { get; init; }
    public required float Linear { get; init; }

    public required float SpecularR { get; init; }
    public required float SpecularG { get; init; }
    public required float SpecularB { get; init; }
    public required float Quadratic { get; init; }
}
