namespace Cubism.Visual;

public sealed class Scene
{
    public List<SceneObject> Roots { get; } = new();
    public DirectionalLight Light { get; set; } = DirectionalLight.Default;

    public void Add(SceneObject obj) => Roots.Add(obj);
}
