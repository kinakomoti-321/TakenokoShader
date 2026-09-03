# Texture Baker

Bakes per-texel maps by ray tracing a renderer hierarchy into a UV layout.
Open it from `Takenoko / Texture Baker`.

## Usage

1. Assign **Bake Target**. Every `MeshRenderer` and `SkinnedMeshRenderer` under
   that object, including inactive ones, is baked into a single map.
2. Pick the **UV Set** that defines the texture layout.
3. Pick a **Target** and set its options.
4. Set resolution, samples and padding, choose an output path, and press **Bake**.

The output is written as an 8 bit PNG with the value replicated across RGB, and
reimported as linear data with sRGB disabled. Pack it into a channel of a combined
mask with the Texture Packer window.

## Targets

### Thickness

Casts rays into the `-N` hemisphere and normalizes the hit distance.

- **Distance Mode** - `Relative To Bounds` expresses the maximum thickness as a
  percentage of the combined bounding box diagonal, which keeps the setting
  independent of model scale. `Absolute` takes world units instead.
- **Bounds Percent** - default 25.
- **Cone Angle** - half angle of the sampling cone, default 75 degrees. Narrower
  values suppress grazing rays that skim along the surface and produce speckle.

The output is thickness, so **thick reads white**. `Takenoko_StandardFragment`
performs the `1 - x` inversion itself after dividing by `NdotV`, so the map feeds
`_SssThicknessTex` directly. Maps baked in Substance and similar tools use the
opposite convention and must be inverted before use here.

Rays that hit nothing within the maximum distance count as fully thick. An open
surface such as cloth or a leaf therefore bakes solid white, which is correct for
a volume thickness map but means thin walled materials want a target of their own.

## Adding a target

Derive from `BakeTarget`, give it a public parameterless constructor, and it
appears in the window's target list automatically through `TypeCache`.

```csharp
public sealed class MyBakeTarget : BakeTarget
{
    public override string DisplayName => "My Map";
    public override string FileSuffix => "MyMap";

    public override void DrawSettings() { /* IMGUI for options */ }
    public override void Prepare(BakeContext context) { /* resolve settings */ }

    public override float Evaluate(in BakeContext context, Vector3 position, Vector3 normal, uint seed)
    {
        // Called from worker threads: no Unity APIs, no shared mutable state.
    }
}
```

`Evaluate` runs on worker threads, so it must not touch Unity APIs or mutate
shared state. Resolve everything that depends on the scene in `Prepare`.

## Implementation notes

**BVH.** Binned SAH with 12 bins, leaves of at most 4 triangles, depth capped at
64. Nodes are 32 bytes laid out as two `float4`s so the array can be uploaded to a
`StructuredBuffer` unchanged when the GPU path lands. Traversal lives in
`BvhRayCaster` rather than `Bvh` so each worker thread owns its own stack.

**Rasterization.** Coverage is conservative: a texel is claimed when the triangle
comes within half a texel diagonal of its center, not only when the center falls
inside the triangle. Without that, texels straddling an island border stay empty
and leave a dashed seam along every edge.

**UV overlap** is resolved first-write-wins by design. Overlapping layouts are out
of scope, so mirrored UV0 islands will bake from whichever triangle lands first.

**Padding** dilates the result outward so bilinear filtering and mip generation do
not pull the background in along island borders.

**Skinned meshes** bake in their current pose, blend shapes included.

## Limitations

- CPU only. The BVH layout is GPU ready but no compute kernel exists yet.
- Memory scales with resolution: the intermediate sample map holds a world position
  and normal per texel, so 4096 needs roughly 460 MB.
- A mirrored transform reverses triangle winding; this is corrected on import, but
  meshes with inconsistent winding of their own will produce wrong facing tests.
