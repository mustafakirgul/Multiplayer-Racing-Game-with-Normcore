using Normal.Realtime;

public class Race : RealtimeComponent<RaceModel>
{
    public double m_fGameStartTime;
    public float m_fRaceDuration;


    protected override void OnRealtimeModelReplaced(RaceModel previousModel, RaceModel currentModel)
    {
        if (previousModel != null)
        {
            previousModel.gameStartTimeDidChange -= GameTimeChanged;
        }

        if (currentModel != null)
        {
            currentModel.gameStartTimeDidChange += GameTimeChanged;
        }
    }

    public void ChangeGameTime(double Time)
    {
        model.gameStartTime = Time;
    }

    private void GameTimeChanged(RaceModel model, double value)
    {
        m_fGameStartTime = model.gameStartTime;
    }
}