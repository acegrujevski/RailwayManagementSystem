using System.ComponentModel.DataAnnotations;

namespace RailwayMenagmentSystem.Models;

public class TrainLocation
{
    public int Id { get; set; }

    public int TrainId { get; set; }

    public Train Train { get; set; }

    public int StationId { get; set; }

    public Station Station { get; set; }

    public DateTime RecordedAt { get; set; }
}