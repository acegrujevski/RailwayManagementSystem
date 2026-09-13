using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using RailwayMenagmentSystem.Models.Enums;

namespace RailwayMenagmentSystem.Models;

public class Reservation
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public Schedule Schedule { get; set; }

    public string UserId { get; set; }

    public IdentityUser User { get; set; }

    public DateTime ReservationDate { get; set; }

    [Range(1, 10)]
    public int NumberOfSeats { get; set; }

    [Required]
    public ReservationStatus Status { get; set; }
}