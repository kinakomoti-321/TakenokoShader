# パッケージ構成と配布

| パッケージ ID | 配布 | 依存 |
| --- | --- | --- |
| `com.kinankomoti.takenokoshader` | VPM | なし |
| `com.kinankomoti.takenokoshaderarealight` | VPM | TakenokoShader / VRChat Worlds 3.10.x |
| `com.kinankomoti.shaderbindingexporter` | 開発用 UPM | なし（Editor 専用） |

各パッケージに MIT ライセンス本文を含めます。
AreaLight の既存スクリプト、Prefab、マテリアル、LUT の GUID は移動前と同じです。
TakenokoShader の本体を AreaLight / Exporter に依存させず、Worlds SDK がないプロジェクトにも導入できます。

VPM の依存関係は `vpmDependencies`、ダウンロード先は `url` に指定します。
ZIP は `package.json` をルートに含め、一覧の各バージョンに `zipSHA256` を追加します。
仕様は [VPM Packages](https://vcc.docs.vrchat.com/vpm/packages/) と
[VPM Repos](https://vcc.docs.vrchat.com/vpm/repos/) を参照してください。

## ローカルで配布物を作る

Unity でパッケージを認識させ、C# / UdonSharp のコンパイルを確認してから実行します。
Python 3.10 以降を使用します。追加の Python ライブラリは不要です。

```powershell
python Tools~/build_packages.py --tag v0.1.0
```

`Build/Packages/` に本体と AreaLight の ZIP、Exporter の UPM tarball（`.tgz`）、
VPM 配布一覧 `index.json` を生成します。Exporter は VPM 一覧に含めません。
新しい配布バージョンでは、変更したパッケージの `package.json` の `version` を更新してください。
3 パッケージのバージョンはそれぞれ管理できます。変更のない配布済みパッケージは、
前回の一覧を渡すことで元のダウンロード先とバージョンを維持します。
依存範囲を超える場合は AreaLight の `vpmDependencies` も更新してください。

過去の配布バージョンを維持する場合:

```powershell
python Tools~/build_packages.py --tag v0.1.1 --previous-index Build/previous-index.json
```

同じパッケージバージョンの配布物を差し替えるとビルドを失敗させます。
配布済みバージョンを変更せず、新しいバージョンを発行してください。

## GitHub Release / VCC

リポジトリルートの `.github/workflows/release-packages.yml` は、`v*` タグの push で
上記の配布物を生成し、GitHub Release に添付します。
前回の `index.json` を取得して過去のバージョンを維持します。
配布先リポジトリは workflow の `github.repository` から取得します。
初回のみ、前回の一覧が存在しない 404 を許容します。

最初の Release 作成後、VCC の **Settings > Packages > Add Repository** に次の URL を登録します。

```text
https://github.com/kinakomoti-321/TakenokoShader/releases/latest/download/index.json
```

この URL は初回 Release を公開するまで利用できません。
安定版タグ（`v0.1.0` など）で運用し、最新の Release に必ず一覧を添付してください。
GitHub Pages の設定は不要です。

ローカルで VPM 形式を確認する場合は VCC の **Settings > Packages > User Packages** に
各パッケージフォルダを追加できます。

## 既存プロジェクトの更新

LightSystem を利用しているプロジェクトでは、本体の更新と同時に AreaLight を導入してください。
新しい本体だけを導入すると、分離された UdonSharp コンポーネントが見つからなくなります。
シーンや Prefab の GUID 参照は継続し、UdonSharp のアセンブリ登録も AreaLight 側へ移動しています。
更新後に UdonSharp のプログラムを再コンパイルしてください。

ShaderBindingExporter は開発環境にのみ導入します。
本リポジトリでは 3 パッケージを `Packages/` の embedded package として開発します。
