# TakenokoShaderAreaLight

TakenokoShader の VRChat ワールド向け UdonSharp エリアライトシステムです。

## 依存パッケージ

- `com.kinankomoti.takenokoshader` 0.1.x
- `com.vrchat.worlds` 3.10.x（UdonSharp を含む）

依存バージョンは Unity 2022.3 / VRChat Worlds SDK 3.10.4 の開発環境に合わせています。
VPM で導入すると依存パッケージも解決されます。

## 使用方法

1. `TakenokoLightSystem.prefab` をシーンに配置します。
2. `TakenokoAreaLight.prefab` を配置します。
3. LightSystem の `Light Slots` に AreaLight を登録します（最大 8 個）。
4. ライトを受ける TakenokoShader マテリアルのエリアライト設定を有効にします。

LUT は `Resource/`、スクリプトは `Runtime/LightSystem/` に含まれます。
Prefab と `AreaLight.mat` は TakenokoShader を使用します。

既存アセットの GUID とスクリプトの名前空間 `Kinankomoti.TakenokoShader` は維持しています。
旧 TakenokoShader から更新する場合は、本体とこのパッケージを同時に更新し、
UdonSharp のプログラムを再コンパイルしてください。

MIT License。ライセンス本文は [LICENSE.md](LICENSE.md) に含まれます。
