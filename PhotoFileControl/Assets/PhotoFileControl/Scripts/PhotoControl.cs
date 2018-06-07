using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
#if UNITY_UWP
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Graphics.Imaging;
#elif UNITY_EDITOR || UNITY_STANDALONE
#endif

namespace PhotoFileControl
{
    public class PhotoControl
    {
        public static IEnumerator ViewPhotos(Action<string[]> action)
        {
            List<string> list = new List<string>();
#if UNITY_UWP
            Task task = Task.Run(async() =>
            {
                var storagefilelist = await KnownFolders.CameraRoll.GetFilesAsync();
                for (int i = 0; i < storagefilelist.Count; i++)
                {
                    list.Add(storagefilelist[i].Name);
                }
            });
            yield return new WaitWhile(() => task.IsCompleted == false);
#elif UNITY_EDITOR || UNITY_STANDALONE
            var path = FileIOControl.LocalFolderPath;
            Thread thread = new Thread(() => {
                var dir=Directory.GetFiles(path);
                for (int i = 0; i < dir.Length; i++)
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

        public static IEnumerator SavePhoto(string name, Texture2D tex,Action action)
        {
            var data = tex.EncodeToPNG();
#if UNITY_UWP
            Task task = Task.Run(async () =>
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

        public static IEnumerator LoadPhoto(string name,Action<Texture2D> action)
        {
            Texture2D tex = null;
            byte[] data = null;
#if UNITY_UWP
            uint w = 0, h = 0;
            Task task = Task.Run(async () =>
            {
                var storagefilelist = await KnownFolders.CameraRoll.GetFilesAsync();
                foreach (var item in storagefilelist)
                {
                    if (item.Name == name)
                    {
                        var img = await item.OpenReadAsync();
                        var decoder = await BitmapDecoder.CreateAsync(img);
                        var pixel = await decoder.GetPixelDataAsync();
                        var bytes = pixel.DetachPixelData();
                        w = decoder.PixelWidth;
                        h = decoder.PixelHeight;
                        data = new byte[bytes.Length];
                        for (int i = 0; i < h; i++)
                        {
                            for (int j = 0; j < w; j++)
                            {
                                data[(w * (h - 1 - i) + j) * 4 + 0] = bytes[(w * i + j) * 4 + 0];
                                data[(w * (h - 1 - i) + j) * 4 + 1] = bytes[(w * i + j) * 4 + 1];
                                data[(w * (h - 1 - i) + j) * 4 + 2] = bytes[(w * i + j) * 4 + 2];
                                data[(w * (h - 1 - i) + j) * 4 + 3] = bytes[(w * i + j) * 4 + 3];
                            }
                        }
                        break;
                    }
                }
            });
            yield return new WaitWhile(() => task.IsCompleted == false);
            tex = new Texture2D((int)w, (int)h, TextureFormat.BGRA32, false);
            tex.LoadRawTextureData(data);
#elif UNITY_EDITOR || UNITY_STANDALONE
            tex = new Texture2D(1, 1);
            yield return FileIOControl.ReadBytesFile(FileIOControl.LocalFolderPath + "\\" + name, (b) => { data = b; });
            tex.LoadImage(data);
#endif
            tex.Apply();
            yield return null;
            if (action != null) action.Invoke(tex);
        }
    }
}
