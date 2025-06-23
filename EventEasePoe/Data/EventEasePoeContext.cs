using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventEasePoe.Models;

namespace EventEasePoe.Data
{
    public class EventEasePoeContext : DbContext
    {
        public EventEasePoeContext (DbContextOptions<EventEasePoeContext> options)
            : base(options)
        {
        }

        public DbSet<EventEasePoe.Models.Event> Event { get; set; } = default!;
        public DbSet<EventEasePoe.Models.Venue> Venue { get; set; } = default!;
        public DbSet<EventEasePoe.Models.Booking> Booking { get; set; } = default!;
    }
}
