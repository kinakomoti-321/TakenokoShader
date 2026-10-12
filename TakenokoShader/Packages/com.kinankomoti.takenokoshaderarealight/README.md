# TakenokoShaderAreaLight

TakenokoShader の VRChat ワールド専用 UdonSharp エリアライトシステムです。Avatar プロジェクトには対応していません。

## 依存パッケージ

- `com.kinankomoti.takenokoshader` 0.1.x
- `com.vrchat.worlds` 3.10.x（UdonSharp を含む）

依存バージョンは Unity 2022.3 / VRChat Worlds SDK 3.10.4 の開発環境に合わせています。
本体の `.unitypackage` と Worlds SDK を先に導入してから、
AreaLight の `.unitypackage` をインポートしてください。

## 使用方法

1. Hierarchy を右クリックし、`TakenokoShader/AreaLightSystem` を選びます。
2. 同じメニューの `TakenokoShader/TakenokoAreaLight` でライトを作成します。
3. LightSystem の `Light Slots` に AreaLight を登録します（最大 8 個）。
4. ライトを受ける TakenokoShader マテリアルのエリアライト設定を有効にします。

Transform・Color・Texture は、変化した項目だけを更新します。
GameObject または UdonBehaviour を無効にすると、そのライトの寄与も無効になります。
各スロットの状態は `_UdonIsActiveAreaLight1`〜`8` に float の `0`（無効）/ `1`（有効）で渡します。

AreaLight の `IsVertexUpdated`・`IsLightColorUpdated`・`IsLightTextureUpdated` は、
LightSystem が全スロットへ反映するまで保持されます。
同じライトを複数スロットへ登録した場合も、各スロットへ変更が反映されます。

UdonSharp からは `SetAreaLight(light, index)` で登録し、`ClearAreaLight(index)` で解除できます。
`index` は `0`〜`7` です。登録時は更新フラグに関係なく現在の全項目を即時反映します。
解除時はスロットの Shader データをクリアし、Active を `0` にします。
`SetAreaLight(null, index)` も解除として扱います。

LightSystem は、同時に読み込まれるシーン全体で最大 1 個にしてください。
右クリックメニューは既存の LightSystem があればそれを選択し、重複作成しません。
無効なオブジェクトも含めて重複を Editor で検出し、VRChat ワールドのビルドを中止します。
この検証は実行中の LightSystem の複製を防ぐものではありません。

オブジェクトを右クリックした場合は、その子として作成されます。
Prefab の参照と初期設定を引き継ぎ、Undo で作成を取り消せます。

LUT は `Resource/`、スクリプトは `Runtime/LightSystem/` に含まれます。
LUT は配置時・編集時に自動設定されるため、Inspector で指定する必要はありません。
Prefab と `AreaLight.mat` は TakenokoShader を使用します。

既存アセットの GUID とスクリプトの名前空間 `Kinankomoti.TakenokoShader` は維持しています。
旧 TakenokoShader から更新する場合は、本体とこのパッケージを同時に更新し、
UdonSharp のプログラムを再コンパイルしてください。

MIT License。ライセンス本文は [LICENSE.md](LICENSE.md) に含まれます。
