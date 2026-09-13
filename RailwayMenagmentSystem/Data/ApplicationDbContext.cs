using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RailwayMenagmentSystem.Models;
using Route = RailwayMenagmentSystem.Models.Route;

namespace RailwayMenagmentSystem.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Train> Trains { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<TrainLocation> TrainLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Route>()
            .HasOne(r => r.DepartureStation)
            .WithMany(s => s.DepartureRoutes)
            .HasForeignKey(r => r.DepartureStationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Route>()
            .HasOne(r => r.ArrivalStation)
            .WithMany(s => s.ArrivalRoutes)
            .HasForeignKey(r => r.ArrivalStationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}