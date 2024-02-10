using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

[CustomTimelineEditor(typeof(EnemyWavesClip))]
public class EnemyWavesClipEditor : ClipEditor
{
    static readonly Dictionary<PreviousSpawnsHandlingMode, Color> ribbonColors = new ()
    {
        { PreviousSpawnsHandlingMode.IncludeInNewCount, Color.yellow },
        { PreviousSpawnsHandlingMode.ForgetSpawnedEnemies, Color.blue },
        { PreviousSpawnsHandlingMode.DespawnEverything, Color.red },
    };


    public override void OnClipChanged(TimelineClip clip)
    {
        var wavesClip = clip.asset as EnemyWavesClip;
        if (wavesClip.WaveData == null)
            return;

        if (wavesClip.WaveData.EnemyCounts.Count == 0)
            clip.displayName = "Empty wave";
        else
        {
            StringBuilder sb = new();
            foreach (var wave in wavesClip.WaveData.EnemyCounts)
                sb.Append($"{wave.Key} ({wave.Value}), ");
            sb.Remove(sb.Length - 2, 2);
            clip.displayName = sb.ToString();
        }
    }

    public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
    {
        var wavesClip = clip.asset as EnemyWavesClip;
        EditorGUI.DrawRect(new Rect(region.position.position, new Vector2(6, region.position.height)), 
            ribbonColors[wavesClip.WaveData.PreviousSpawnsHandlingMode]);
        base.DrawBackground(clip, region);
    }
}