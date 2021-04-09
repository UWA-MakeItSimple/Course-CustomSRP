using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

partial class CameraRenderer
{
#if UNITY_EDITOR
    string SampleName { get; set; }
#else
    const string SampleName = bufferName;
#endif

#if UNITY_EDITOR
    static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    static Material errorMaterial;

    void DrawGizmos()
    {
        if(Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }

    void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera));
        drawingSettings.overrideMaterial = errorMaterial;
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResoults, ref drawingSettings, ref filteringSettings);
    }

    void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
    }

    void PrepareBuffer()
    {
        buffer.name = SampleName = camera.name;
    }
#endif
}