using OpenTK.Mathematics;

namespace NetCraft.Models.Lights;

public class PointLight
{
    public required Vector3 Position { get; set; }
    public required Vector3 Ambient { get; set; }
    public required Vector3 Specular { get; set; }
    public required Vector3 Color { get; set; }

    public required float Intensity { get; set; }

    public required float Constant { get; set; }
    public required float Linear { get; set; }
    public required float Quadratic { get; set; }
}
