using System.ComponentModel.DataAnnotations;

namespace RailwayMenagmentSystem.Models;

public class Schedule
{
    public int Id { get; set; }

    public int TrainId { get; set; }

    public Train? Train { get; set; }

    public int RouteId { get; set; }

    public Route? Route { get; set; }

    [Required]
    public DateTime DepartureTime { get; set; }

    [Required]
    public DateTime ArrivalTime { get; set; }

    [Range(1, 100000)]
    public decimal Price { get; set; }

    [Range(0, 1000)]
    public int AvailableSeats { get; set; }

    public ICollection<Reservation>? Reservations { get; set; }
}