using OpenTK.Graphics.OpenGL4;

namespace NetCraft.Utils;

public class ModelManager
{
    private ModelManager() { }

    private Dictionary<string, EntityModel> models = new();

    public IReadOnlyDictionary<string, EntityModel> Models => models.AsReadOnly();
    public static ModelManager Instance = new();

    public static void InitializeModel()
    {
        EntityModel model = new("Resources/cube.obj", 0, 0);
        Instance.models.Add("simpleVoxel", model);
    }
}
