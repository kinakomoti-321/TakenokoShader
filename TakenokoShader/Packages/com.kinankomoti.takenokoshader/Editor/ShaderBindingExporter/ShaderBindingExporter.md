# ShaderBindingExporter
Shader の property、および shader keyword の名前を取得し、型として記入できるように custom editor 向けの型を定義する C# script を出力するツールです

## Type
文字列による binding の指定は補完が効かない、人的ミスの原因となるなどの問題があるため、これを防止すべく各 binding と同名の変数を用意することを目的としています。
現在の仕様では Enum 型で binding を用意し、対応する string を description attribute と ~Name クラスによって対応付けています。

```
public enum ShaderProps
{
    [Description("_LitFactor")]
    LitFactor,
    [Description("_MainTex")]
    MainTex,
    [Description("_LitColor")]
    LitColor,
    [Description("_ShadeColor")]
    ShadeColor,
    ...
}

public static class ShaderPropsName
{
    public static readonly Dictionary<ShaderProps, string> NameTable = new Dictionary<ShaderProps, string>
    {
        { ShaderProps.LitFactor, "_LitFactor" },
        { ShaderProps.MainTex, "_MainTex" },
        { ShaderProps.LitColor, "_LitColor" },
        { ShaderProps.ShadeColor, "_ShadeColor" },
        { ShaderProps.ShadeTex, "_ShadeTex" },
        ...
    }
}
```

また、description attribute からの取得を容易にさせるため、`Name` メソッドによって呼び出せるようにEnum extension を追加する script も出力できるようになっています。
```
    public static class EnumExtension
    {
        public static string Name(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attr?.Description ?? value.ToString();
        }
    }
```

## Usage
- Shader : 対象の shader 
- Bindings : shader の binding 一覧を確認できます
- Namespace : 出力するコードの namespcae です。shader アタッチ時に自動的に名前からデフォルト値を決定します。手動でも設定可能です。
- Shader property : property 用の型を出力
- Shader keyword : keyword 用の型を出力
- Enum extension : Description attribute 向けの enum extenion を出力します
- Output path : 出力先のパス
- Enable aut export : 自動的に shader の更新を判定し、出力する機能
- Export : 以上の config を基に script を出力します