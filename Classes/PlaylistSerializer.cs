using Models;
using ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Classes
{
    public static class PlaylistSerializer
    {
        public static void Serialize(string filename, List<PlaylistItem> items)
        {
            using (Stream stream = File.Open(filename, FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, items.ToList());
            }
        }

        public static void Load(PlaylistViewModel VM, string filename)
        {
            if (!File.Exists(filename))
                return;

            VM.Tracks.Clear();

            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();

                try
                {
                    var items = (List<PlaylistItem>)bin.Deserialize(stream);

                    foreach (PlaylistItem item in items)
                    {
                        VM.Tracks.Add(item);
                    }
                }
                catch (Exception exc)
                {
                    return;
                }
            }
            PlaylistItem playitem = VM.Tracks.FirstOrDefault(t => t.IsPlaying);
            if (playitem != null)
                VM.CurrentPlayPosition = playitem.Position;

            VM.Raise("Tracks");
        }
    }
}
