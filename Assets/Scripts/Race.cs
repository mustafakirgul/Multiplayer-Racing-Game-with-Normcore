using Normal.Realtime;

public class Race : RealtimeComponent<RaceModel>
{
    public double m_fGameStartTime;
    public float m_fRaceDuration;
    public int m_iPhase;
    public bool m_isOn;


    protected override void OnRealtimeModelReplaced(RaceModel previousModel, RaceModel currentModel)
    {
        if (previousModel != null)
        {
            previousModel.gameStartTimeDidChange -= GameTimeChanged;
            previousModel.phaseDidChange -= PhaseChanged;
            previousModel.isOnDidChange -= IsOnChanged;
        }

        if (currentModel != null)
        {
            if (currentModel.isFreshModel)
            {
                m_iPhase = currentModel.phase;
                m_isOn = currentModel.isOn;
            }

            currentModel.gameStartTimeDidChange += GameTimeChanged;
            currentModel.phaseDidChange += PhaseChanged;
            currentModel.isOnDidChange += IsOnChanged;
        }
    }

    public void ChangeIsOn(bool state)
    {
        model.isOn = state;
    }

    public void ChangePhase(int phase)
    {
        model.phase = phase;
    }

    private void PhaseChanged(RaceModel raceModel, int phase)
    {
        m_iPhase = phase;
        GameManager.instance.phaseManager.JumpToPhase(phase);
    }

    public void ChangeGameTime(double time)
    {
        model.gameStartTime = time;
    }

    private void GameTimeChanged(RaceModel raceModel, double value)
    {
        m_fGameStartTime = value;
    }

    private void IsOnChanged(RaceModel raceModel, bool value)
    {
        FindObjectOfType<TopRacersLive>().isRunning = value;
        m_isOn = value;
    }
}