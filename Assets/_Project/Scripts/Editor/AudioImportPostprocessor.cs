using UnityEditor;
using UnityEngine;

namespace Farm.Editor
{
    // The music tracks (Resources/Music) are long: they stream from disk instead of being unpacked into memory, and are stored as Vorbis in the
    // player (the source files stay as they were made).
    public sealed class AudioImportPostprocessor : AssetPostprocessor
    {
        const string MusicRoot = "Assets/_Project/Resources/Music/";

        public override uint GetVersion() => 1;

        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(MusicRoot)) return;
            var importer = (AudioImporter)assetImporter;
            importer.forceToMono = false;
            importer.loadInBackground = true;
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.6f;
            importer.defaultSampleSettings = settings;
        }
    }
}
