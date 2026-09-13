using System.ComponentModel.DataAnnotations;
using RailwayMenagmentSystem.Models.Enums;

namespace RailwayMenagmentSystem.Models;

public class Train
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Model { get; set; }

    [Range(1, 1000)]
    public int Capacity { get; set; }

    [Required]
    public TrainStatus Status { get; set; }

    public ICollection<Schedule> Schedules { get; set; }

    public ICollection<TrainLocation> TrainLocations { get; set; }
}