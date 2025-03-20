public class Cities
{
    private String name;
    private double x;
    private double y;
    private List<Cities> linkedTo;
    public Cities(String name,double x , double y , List<Cities> linkedTo)
    {
        this.name = name;
        this.x = x;
        this.y = y;
        this.linkedTo = linkedTo;
    }
    public String GetName()
    {
        return name;
    }
    public double GetX()
    {
        return x;
    }
    public double GetY()
    {
        return y;
    }
    public List<Cities> GetLinkedTo()
    {
        return linkedTo;
    }

    public void setName(String name)
    {
        this.name = name;
    }


}