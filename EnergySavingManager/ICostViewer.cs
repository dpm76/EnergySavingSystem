namespace EnergySavingManager
{
    /// <summary>
    /// Interfaz para los visores del coste
    /// </summary>
    public interface ICostViewer
    {
        string SourceId { get; set; }
        float CommitedPowerCost { set; }
        float EstimatedPowerCost { set; }
        float EstimatedPowerCostSaved { set; }
    }
}