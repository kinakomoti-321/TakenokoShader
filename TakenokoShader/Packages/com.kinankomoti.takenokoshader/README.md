# TakenokoShader

TakenokoShader は kinankomoti が作成している Unity 向けのシェーダーパッケージのひな形です。

## 目次

- 概要
- インストール
- 使い方
- 参考

## 概要

このパッケージは、Unity の UPM パッケージとして使える最小構成のテンプレートです。
今回は「とりあえず動く形」を作ることを目的に、Runtime と Editor の基本構成を用意しています。

## インストール

1. このフォルダを Packages 配下に置く
2. Unity が package.json を認識することを確認する
3. 必要に応じて Package Manager から表示を確認する

## 使い方

- Runtime 側にシェーダー定義や共通スクリプトを追加する
- Editor 側にインスペクタ拡張やツールを追加する
- 必要に応じて Samples～ でサンプルを追加する

## 参考

- `com.kinankomoti.takenokoshader`
- `TakenokoShaderSettings`
- `TakenokoShader_Default.shader`
