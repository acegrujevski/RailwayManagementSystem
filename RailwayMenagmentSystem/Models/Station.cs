using System.ComponentModel.DataAnnotations;

namespace RailwayMenagmentSystem.Models;

public class Station
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string Address { get; set; }

    public ICollection<Route>? DepartureRoutes { get; set; }

    public ICollection<Route>? ArrivalRoutes { get; set; }

    public ICollection<TrainLocation>? TrainLocations { get; set; }
}