using System;
using UnityEngine;
using UnityEngine.Rendering;

partial class CameraRenderer
{
    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer() { name = bufferName };
    CullingResults cullingResoults;

    ScriptableRenderContext context;
    Camera camera;

    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing)
    {
        this.context = context;
        this.camera = camera;
        buffer.name = SampleName = camera.name;

#if UNITY_EDITOR
        PrepareForSceneWindow();
#endif

        if (!Cull())
            return;

        SetUp();
        DrawVisiableGeometry(useDynamicBatching, useGPUInstancing);
#if UNITY_EDITOR
        DrawUnsupportedShaders();
        DrawGizmos();
#endif
        Submit();
    }

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    private void DrawVisiableGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        //Draw Opaque
        var sortingSettings = new SortingSettings(camera);
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        drawingSettings.enableDynamicBatching = useDynamicBatching;
        drawingSettings.enableInstancing = useGPUInstancing;
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(cullingResoults, ref drawingSettings, ref filteringSettings);
        //Draw Skybox
        context.DrawSkybox(camera);
        //Draw Transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResoults, ref drawingSettings, ref filteringSettings);
    }

    bool Cull()
    { 
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResoults = context.Cull(ref p);
            return true;
        }
        return false;
    }

    private void SetUp()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    private void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

}