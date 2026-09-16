using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using RailwayMenagmentSystem.Models.Enums;

namespace RailwayMenagmentSystem.Models;

public class Reservation
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }
    [ValidateNever]
    public Schedule Schedule { get; set; }

    [ValidateNever]
    public string UserId { get; set; }

    [ValidateNever]
    public IdentityUser User { get; set; }

    public DateTime ReservationDate { get; set; }

    [Range(1, 10)]
    public int NumberOfSeats { get; set; }

    [ValidateNever]
    public ReservationStatus Status { get; set; }
}