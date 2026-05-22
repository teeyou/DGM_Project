public static class ColorTable
{
    public static string White => "#FFFFFF";
    public static string Green => "#00FF00";
    public static string Sky => "#00C0FF";
    public static string Red => "#FF0000";
    public static string Yellow => "#FFFF00";
    public static string Purple => "#7A00FF";

    public static string GetColor(string attr)
    {
        return attr switch
        {
            "Vaccine" => ColorTable.Green,
            "Va" => ColorTable.Green,
            
            "Data" => ColorTable.Sky,
            "Da" => ColorTable.Sky,
            
            "Virus" => ColorTable.Red,
            "Vi" => ColorTable.Red,

            "Unknown" => ColorTable.Purple,
            "Un" => ColorTable.Purple,
            "Uk" => ColorTable.Purple,

            "Free" => ColorTable.Yellow,
            "Fr" => ColorTable.Yellow,

            _ => ColorTable.White
        };
    }
}
