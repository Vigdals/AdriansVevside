namespace Adrians.Models;

public sealed class SimasTommekalenderItem
{
    public int FraksjonId { get; set; }

    public List<DateTime> Tommedatoer { get; set; } = [];
}