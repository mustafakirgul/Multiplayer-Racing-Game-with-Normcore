using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class Race : RealtimeComponent<RaceModel>
{
    public double m_fGameStartTime;
    public float m_fRaceDuration;
    public RaceModel _model;

    protected override void OnRealtimeModelReplaced(RaceModel previousModel, RaceModel currentModel)
    {
        if (previousModel != null)
        {
            // Unregister from events
            previousModel.gameStartTimeDidChange -= GameTimeChanged;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                m_fGameStartTime = currentModel.gameStartTime;
            }
            currentModel.gameStartTimeDidChange += GameTimeChanged;

            //Update current model of player when applicable
            Debug.LogWarning("Model updated");
            _model = currentModel;
        }
    }

    public void ChangeGameTime(double Time)
    {
        _model.gameStartTime = Time;
    }

    private void GameTimeChanged(RaceModel model, double value)
    {
        m_fGameStartTime = value;
    }
}
