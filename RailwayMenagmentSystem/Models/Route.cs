using System.ComponentModel.DataAnnotations;

namespace RailwayMenagmentSystem.Models;

public class Route
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public int DepartureStationId { get; set; }

    public Station? DepartureStation { get; set; }

    public int ArrivalStationId { get; set; }

    public Station? ArrivalStation { get; set; }

    public ICollection<Schedule>? Schedules { get; set; }
}