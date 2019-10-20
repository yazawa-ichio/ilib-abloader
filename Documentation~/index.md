# [ilib-abloader](https://github.com/yazawa-ichio/ilib-abloader)

Unity AssetBundle Loader Package.

リポジトリ https://github.com/yazawa-ichio/ilib-abloader

## 概要

コルーチン及びロードにUpdateを使わない形のアセットバンドルのローダーです。  
手軽の利用できるという点ではまあまあ？　縛りプレイで作っただけなので、実用にはイマイチ機能が足りていない。  
個人開発だとこれぐらいで、あとは外でラップしていい感じにフォールバッグやらを付け足したらギリギリ利用できるかなという程度です。  

その他、簡易的にアセットバンドル名を設定できるツールと、ビルド時にアセットバンドルのサイズやCRC情報を収集する専用のビルダーも含みます。

#### [アセットバンドル名設定ツール(WIP)](ab-name-setter.md)

#### [拡張マニフェストビルダー(WIP)](ex-manifest-builder.md)

### 公式のAddressableAssetsSystemとの違い

Addressableがまともに使い物にならない時期に作り始めました。  
今から使う分には公式のAddressableの採用するのはありかと思います。  
ただ、個人的にAddressableはAssetBundleを透過的に扱うために内部がかなり複雑な実装で、ビルドパイプライン一式も含む大きめなフレームワークなので小回りが利きにくいと思います。  
逆にABLoaderはAssetBundleを扱う上で必要な依存解決とロードの参照管理、ダウンロード時のキャッシュ管理に絞っています。  
それなりの規模の開発においては、このパッケージをコピーして用途に合わせてチューニングするのが一番かと思います。  

## セットアップ方法

### 事前準備

ABLoaderを利用するには事前にアセットバンドルをビルド出来る環境を用意する必要があります。  
特にエディタでシミュレート実行(`UseEditorAsset`)する際は`assetBundleName`を設定する必要があります。  

アセットバンドル名は手で付ける事も出来ますが、基本的にインポーター経由で付けるのか適切です。  
このパッケージにはアセットバンドル名を設定できるツールが同梱されているので、必要であれば使ってください。  
ファイルパスをベースでアセットバンドル名を決定し、最低限の設定しかつけていないので複雑なことをは出来ません。  

[アセットバンドル名設定ツール](ab-name-setter.md)

サーバーからダウンロードする形式であれば、ダウンロード時の破損チェックとファイルサイズが必要になります。  
その場合に必要なビルドパイプラインがパッケージに同梱されています。  
通常のアセットバンドルビルドのポスト処理としてファイルサイズやCRCを収集した拡張マニフェストをビルド出来ます。  

[拡張マニフェストビルダー](ex-manifest-builder.md)

### 初期化を行う

初期化時にロード方法を決定する`ILoadOperator`を渡します。  
`ILoadOperator`は自前で実装する事も出来ますが、`StreamingAssets`とサーバー経由の標準的な実装はすでに用意されています。  
`ABLoader.Initialize`関数で初期化してください。

#### StreamingAssetsからロードする

`InternalLoadOperator`を使用します。  
`ABLoader.CreateInternalOperator`関数を利用すると、`StreamingAssets`のパス以下のみの指定で`ILoadOperator`を作成できます。  
引数は以下になります。

* directory

`StreamingAssets`以下のアセットバンドルが入っているディレクトリを指定します。

* manifest

マニフェストのバンドル名を指定します。  
標準ではアセットバンドルのビルド時のディレクトリ名になります。  
※ExManifestを使う場合は別になります。

* manifestAssetName 

マニフェストのアセット名を指定します。  
標準のアセットバンドルのマニフェストでは`AssetBundleManifest`を指定します。  
※ExManifestを使う場合は別になります。

#### サーバーからダウンロードしてからロードする

`NetworkLoadOperator`を使用します。  
`ABLoader.CreateLoadOperator`関数を利用すると、`Application.temporaryCachePath+"/AssetBundles"`以下にキャッシュを保存する`ILoadOperator`を作成できます。  

* url

ダウンロードのベースとなるURLです。

* manifest

マニフェストのバンドル名を指定します。  
標準ではアセットバンドルのビルド時のディレクトリ名になります。  
※ExManifestを使う場合は別になります。  

* version

マニフェストのバージョンです。  
保存したくない場合などは日時等にしておくと毎回ダウンロードされます。

* manifestAssetName 

マニフェストのアセット名を指定します。  
標準のアセットバンドルのマニフェストでは`AssetBundleManifest`を指定します。  
※ExManifestを使う場合は別になります。

#### エディタ上でテストする

`ABLoader.UseEditorMode = true`にします。  
エディタ上において`AssetDataBase`経由で直接アセットをロードします。  

## アセットをロードする

### LoadContainer

アセットバンドルへの参照を持つコンテナをロードします。  
ロード完了時、コンテナへの参照クラスがコールバックで返されます。  
アセットバンドルはこのコンテナの参照クラスを通して、参照カウントを持っており、すべての参照がなくなった際にアンロードされます。  
デフォルトではGCによって解放された際に、自動的に参照カウントを減らします。  
後述の`LoadAsset`等は、この関数を利用したエイリアス実装になります。  

```csharp
using ILib.AssetBundles;

void Prop()
{
	//アセットバンドルのファイル名を指定する
	ABLoader.LoadContainer("texture/test1.bundle",  container => {
		//コンテナからテクスチャをロード
		var texture = container.LoadAsset<Texture2D>("Test1");

		//非同期メソッドでも呼べる
		container.LoadAssetAsync<Texture2D>("Test1",(tex) => {
			
		});

		//GCで自動で破棄されるが、手動で呼ぶことが出来る。
		container.Dispose();
	}, ex => {
		//例外を通知
		Alert.Throw(ex);
	});
}
```

### LoadAsset/LoadAssetAsync

アセットバンドル名とアセット名を指定してアセットをロードします。  
アセットバンドルの参照は自動で解放されます。  

```csharp
using ILib.AssetBundles;

void Prop()
{
	//アセットバンドルのファイル名を指定する
	ABLoader.LoadAssetAsync<Texture2D>("texture/test1.bundle", "Test1", tex => {
		//テクスチャをキャッシュするなり、利用するなりする
	}, ex => {
		//例外を通知
		Alert.Throw(ex);
	});
}
```

### LoadScene/LoadSceneAsync

アセットバンドル名とアセット名を指定してアセットをロードします。  
アセットバンドルの参照は自動で解放されます。  

```csharp
using ILib.AssetBundles;

void Prop()
{
	//アセットバンドルのファイル名を指定する
	ABLoader.LoadSceneAsync("scene/test1.bundle", "Test1", ()) => {
		//シーンがロード出来た。
	}, ex => {
		//例外を通知
		Alert.Throw(ex);
	});
}
```

### 事前にダウンロードを行う

`ABLoader.Download`関数でダウンロードのみを行うことが出来ます。  
返り値からダウンロードの進捗を取得することも出来ます。  

```csharp
using UnityEngine.UI;
using ILib.AssetBundles;

IEnumerator Prop(Text progressDisp, System.Action onSuccess)
{
	var names = new string[] { "texture/test1.bundle", "texture/test2.bundle", "texture/test3.bundle" };
	var complete = false;
	var success = false;
	var fails = new List<System.Exception>();

	//ExManifestを利用するとダウンロードサイズが取得できる
	//var size = ABLoader.GetDownloadSize(names);

	//進捗が0～1fの間で取れるFunc<float>が返る
	var progress = ABLoader.Download(names, ret => {
		complete = true;
		success = ret;
	}, ex => {
		fails.Add(ex);
	});
	
	progressDisp.text = "0%";
	while(!complete)
	{
		var percentage = (int)(progress() * 100f);
		progressDisp.text = $"{percentage}%";
		yield reyurn null;
	}
	progressDisp.text = "100%";

	if(success)
	{
		onSuccess?.Invoke();
	}
	else
	{
		//例外を通知
		//実際には数回リトライした方がいい。
		Alert.Throw(fails.ToArray());
	}
}

```

## 処理を終了する

再起動時などはロード処理をすべて中断する必要があります。  
`ABLoader.Stop`関数で全てのロード処理を中断し、リセット処理を行えます。  
ただし、リクエスト元に中断は通知されません。  
停止後は再度、初期化を行う必要があります。  

```csharp
using ILib.AssetBundles;

void Prop()
{
	//キャッシュクリア時も安全のため全ての処理を中断してから行います。
	ABLoader.Stop(()=>{
		//再起動
		Reboot.Run();
	});
}
```

### キャッシュクリア

ネットワーク経由でダウンロードを行った場合、キャッシュをクリアできる必要があります。  
`ABLoader.CacheClear`関数でキャッシュクリアが実行できます。  
安全のため内部的に`ABLoader.Stop`を実行した後にキャッシュクリアが実行されます。  
そのため、再度初期化を行う必要があります。

```csharp
using ILib.AssetBundles;

void Prop()
{
	//キャッシュクリア時も安全のため全ての処理を中断してから行います。
	ABLoader.CacheClear(()=>{
		//再起動
		Reboot.Run();
	});
}
```

## LICENSE

https://github.com/yazawa-ichio/ilib-abloader/blob/master/LICENSE