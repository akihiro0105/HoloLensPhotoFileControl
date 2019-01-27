using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_UWP
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Graphics.Imaging;
#elif UNITY_EDITOR || UNITY_STANDALONE
using System.IO;
using System.Threading;
using HoloLensModule.Environment;
#endif

namespace PhotoFileControl
{
    /// <summary>
    /// UWP環境 : Photoフォルダ(カメラロール)に対する画像の取得，保存
    /// Editor環境 : Localフォルダに対する画像の取得，保存
    /// </summary>
    public class PhotoControl
    {
        /// <summary>
        /// Photoフォルダの画像一覧を取得
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerator ViewPhotos(Action<string[]> action)
        {
            var list = new List<string>();
#if UNITY_UWP
            var task = Task.Run(async() =>
            {
                var storagefilelist = await KnownFolders.CameraRoll.GetFilesAsync();
                foreach (var file in storagefilelist) list.Add(file.Name);
            });
            yield return new WaitWhile(() => task.IsCompleted == false);
#elif UNITY_EDITOR || UNITY_STANDALONE
            var path = FileIOControl.LocalFolderPath;
            var thread = new Thread(() =>
            {
                var dir = Directory.GetFiles(path);
                for (var i = 0; i < dir.Length; i++)
                {
                    list.Add(Path.GetFileName(dir[i]));
                }
            });
            thread.Start();
            yield return new WaitWhile(() => thread.IsAlive == true);
#endif
            yield return null;
            if (action != null) action.Invoke(list.ToArray());
        }

        /// <summary>
        /// Photoフォルダにpng画像ファイルを保存
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tex"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerator SavePhoto(string name, Texture2D tex,Action action)
        {
            var data = tex.EncodeToPNG();
#if UNITY_UWP
            var task = Task.Run(async () =>
            {
                var file = await KnownFolders.CameraRoll.CreateFileAsync(name + ".png", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(file, data);
            });
            yield return new WaitWhile(() => task.IsCompleted == false);
#elif UNITY_EDITOR || UNITY_STANDALONE
            yield return FileIOControl.WriteBytesFile(FileIOControl.LocalFolderPath + "\\" + name + ".png", data);
#endif
            yield return null;
            if (action != null) action.Invoke();
        }

        /// <summary>
        /// Photoフォルダから画像ファイルを取得
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerator LoadPhoto(string name,Action<Texture2D> action)
        {
            Texture2D tex = null;
            byte[] data = null;
#if UNITY_UWP
            uint w = 0, h = 0;
            var task = Task.Run(async () =>
            {
                var file = await KnownFolders.CameraRoll.GetFileAsync(name);
                if (file != null)
                {
                    var img = await file.OpenReadAsync();
                    var decoder = await BitmapDecoder.CreateAsync(img);
                    var pixel = await decoder.GetPixelDataAsync();
                    var bytes = pixel.DetachPixelData();
                    w = decoder.PixelWidth;
                    h = decoder.PixelHeight;
                    data = new byte[bytes.Length];
                    // 取得データは上下が反転している
                    for (var i = 0; i < h; i++)
                    {
                        for (var j = 0; j < w; j++)
                        {
                            data[(w * (h - 1 - i) + j) * 4 + 0] = bytes[(w * i + j) * 4 + 0];
                            data[(w * (h - 1 - i) + j) * 4 + 1] = bytes[(w * i + j) * 4 + 1];
                            data[(w * (h - 1 - i) + j) * 4 + 2] = bytes[(w * i + j) * 4 + 2];
                            data[(w * (h - 1 - i) + j) * 4 + 3] = bytes[(w * i + j) * 4 + 3];
                        }
                    }
                }
            });
            yield return new WaitWhile(() => task.IsCompleted == false);
            tex = new Texture2D((int)w, (int)h, TextureFormat.BGRA32, false);
            tex.LoadRawTextureData(data);
#elif UNITY_EDITOR || UNITY_STANDALONE
            tex = new Texture2D(1, 1);
            yield return FileIOControl.ReadBytesFile(FileIOControl.LocalFolderPath + "\\" + name, (b) => data = b);
            tex.LoadImage(data);
#endif
            tex.Apply();
            yield return null;
            if (action != null) action.Invoke(tex);
        }
    }
}
