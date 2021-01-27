using Normal.Realtime;

public class Race : RealtimeComponent<RaceModel>
{
    public double m_fGameStartTime;
    public float m_fRaceDuration;
    public int m_iPhase;


    protected override void OnRealtimeModelReplaced(RaceModel previousModel, RaceModel currentModel)
    {
        if (previousModel != null)
        {
            previousModel.gameStartTimeDidChange -= GameTimeChanged;
            previousModel.phaseDidChange -= PhaseChanged;
        }

        if (currentModel != null)
        {
            currentModel.gameStartTimeDidChange += GameTimeChanged;
            currentModel.phaseDidChange += PhaseChanged;
        }
    }

    public void ChangePhase(int phase)
    {
        model.phase = phase;
    }

    private void PhaseChanged(RaceModel raceModel, int phase)
    {
        m_iPhase = model.phase;
        GameManager.instance.phaseManager.JumpToPhase(phase);
    }

    public void ChangeGameTime(double time)
    {
        model.gameStartTime = time;
    }

    private void GameTimeChanged(RaceModel raceModel, double value)
    {
        m_fGameStartTime = model.gameStartTime;
    }
}