namespace RiftPracticePlus;

public class Strategy {
    public readonly int Score;
    public readonly Activation Activation;
    public readonly Strategy[] NextStrategies;

    public Strategy(int score, Activation activation, Strategy[] nextStrategies) {
        Score = score;
        Activation = activation;
        NextStrategies = nextStrategies;
    }
}