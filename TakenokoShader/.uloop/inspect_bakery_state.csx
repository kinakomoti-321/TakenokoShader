using UnityEngine;
using System.Collections.Generic;

var rows = new List<string>();
foreach (var storage in Resources.FindObjectsOfTypeAll<ftLightmapsStorage>())
{
    if (!storage.gameObject.scene.IsValid() || !storage.gameObject.scene.isLoaded) continue;
    rows.Add("storage=" + storage.name);
    rows.Add("maps=" + storage.maps.Count + ", dirMaps=" + storage.dirMaps.Count);
    rows.Add("rnm=" + storage.rnmMaps0.Count + "/" + storage.rnmMaps1.Count + "/" + storage.rnmMaps2.Count);
    rows.Add("mapsMode=" + (storage.mapsMode == null ? "null" : string.Join(",", storage.mapsMode)));
    rows.Add("renderDirMode=" + storage.renderSettingsRenderDirMode);
    rows.Add("lightmapHasRNM=" + (storage.lightmapHasRNM == null ? "null" : string.Join(",", storage.lightmapHasRNM)));
}

int modeId = Shader.PropertyToID("bakeryLightmapMode");
int monoId = Shader.PropertyToID("_BAKERY_MONOSH");
foreach (var renderer in Resources.FindObjectsOfTypeAll<Renderer>())
{
    if (!renderer.gameObject.scene.IsValid() || !renderer.gameObject.scene.isLoaded) continue;
    var block = new MaterialPropertyBlock();
    renderer.GetPropertyBlock(block);
    var material = renderer.sharedMaterial;
    rows.Add("renderer=" + renderer.name + ", lm=" + renderer.lightmapIndex + ", mode=" + block.GetFloat(modeId) + ", monoFloat=" + (material == null ? -1 : material.GetFloat(monoId)) + ", hasMonoKeyword=" + (material != null && material.IsKeywordEnabled("BAKERY_MONOSH")) + ", shader=" + (material == null ? "null" : material.shader.name));
}

return string.Join("; ", rows);
