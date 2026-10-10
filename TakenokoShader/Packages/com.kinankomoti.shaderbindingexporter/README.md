# ShaderBindingExporter

シェーダーのプロパティとキーワードから、C# の enum、名前テーブル、enum 拡張を生成する開発用ツールです。
Unity 2022.3 以降の Editor で動作します。TakenokoShader、UdonSharp、テストフレームワークへの依存はありません。

## インストール

このリポジトリの開発環境では `Packages/` 内の embedded package として使用します。
別のローカル開発環境では、Unity Package Manager の **Add package from disk** から
このフォルダの `package.json` を選択できます。

## 使用方法

**Tools > Shader Binding Exporter** を開き、Shader、Namespace、Output path と生成対象を指定して **Export** を押します。
詳しい設定と生成コード例は [ShaderBindingExporter.md](ShaderBindingExporter.md) を参照してください。

Editor 専用の開発用パッケージです。配布物を生成せず、VPM 一覧と GitHub Release の添付対象から除外します。
生成済みの C# コードを利用するプロジェクトでは、このパッケージのインストールは不要です。

MIT License。ライセンス本文は [LICENSE.md](LICENSE.md) に含まれます。
