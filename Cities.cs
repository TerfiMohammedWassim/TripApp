public class Cities
{
    private string name;
    private double x;
    private double y;
    private Dictionary<Cities, int> goingTo;
    private bool isStarting;
    private bool isTarget;

    public Cities(string name, double x, double y, Dictionary<Cities, int> goingTo, bool isStarting)
    {
        this.name = name;
        this.x = x;
        this.y = y;
        this.goingTo = goingTo;
        this.isStarting = isStarting;
        this.isTarget = false;
    }

    public string GetName() { return name; }
    public void setName(string name) { this.name = name; }
    public double GetX() { return x; }
    public double GetY() { return y; }
    public Dictionary<Cities, int> GetGoingTo() { return goingTo; }
    public bool IsStarting() { return isStarting; }
    public bool IsTarget() { return isTarget; }
    public void SetIsStarting(bool isStarting) { this.isStarting = isStarting; }
    public void SetIsTarget(bool isTarget) { this.isTarget = isTarget; }
}