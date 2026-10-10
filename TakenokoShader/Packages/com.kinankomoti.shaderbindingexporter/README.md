# ShaderBindingExporter

シェーダーのプロパティとキーワードから、C# の enum、名前テーブル、enum 拡張を生成する開発用ツールです。
Unity 2022.3 以降の Editor で動作します。TakenokoShader、UdonSharp、テストフレームワークへの依存はありません。

## インストール

Unity Package Manager の **Add package from git URL** で次を指定します。

```text
https://github.com/kinakomoti-321/TakenokoShader.git?path=TakenokoShader/Packages/com.kinankomoti.shaderbindingexporter
```

特定のリリースを使う場合は末尾に `#v0.1.0` のようにタグを追加します。
ローカルでは **Add package from disk** からこのフォルダの `package.json` を選択できます。
Release の `.tgz` は **Add package from tarball** でインストールできます。

## 使用方法

**Tools > Shader Binding Exporter** を開き、Shader、Namespace、Output path と生成対象を指定して **Export** を押します。
詳しい設定と生成コード例は [ShaderBindingExporter.md](ShaderBindingExporter.md) を参照してください。

Editor 専用パッケージで、VPM の配布一覧には含めません。
生成済みの C# コードを利用するプロジェクトでは、このパッケージのインストールは不要です。

MIT License。ライセンス本文は [LICENSE.md](LICENSE.md) に含まれます。
