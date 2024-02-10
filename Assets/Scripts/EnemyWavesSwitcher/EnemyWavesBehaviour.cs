using UnityEngine.Playables;

public class EnemyWavesBehaviour : PlayableBehaviour
{
    public WaveData WaveData;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var enemyController = playerData as EnemyController;
        if (enemyController.LastWaveData == WaveData)
            return;

        enemyController.AssignWaveData(WaveData);
    }
}
