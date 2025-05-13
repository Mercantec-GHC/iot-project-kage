using IotProject.Shared.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace IotProject.API.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceData> DeviceData { get; set; }
        public DbSet<DeviceConfig> DeviceConfigs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.Owner)
                .WithMany(u => u.Rooms)
                .HasForeignKey(r => r.OwnerId)
                .HasPrincipalKey(u => u.Id);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.Owner)
                .WithMany(u => u.Devices)
                .HasForeignKey(d => d.OwnerId)
                .HasPrincipalKey(u => u.Id);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.Room)
                .WithMany(r => r.Devices)
                .HasForeignKey(d => d.RoomId)
                .HasPrincipalKey(r => r.Id);

            modelBuilder.Entity<DeviceData>()
                .HasOne(dd => dd.Device)
                .WithMany(d => d.Data)
                .HasForeignKey(dd => dd.DeviceId)
                .HasPrincipalKey(d => d.Id);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<DeviceConfig>()
                .HasOne(dc => dc.Device)
                .WithOne(d => d.DeviceConfig)
                .HasForeignKey<DeviceConfig>(dc => dc.DeviceId)
                .HasPrincipalKey<Device>(d => d.Id);

            var converter = new ValueConverter<Dictionary<string, object>, string>(
                dict => JsonSerializer.Serialize(dict, (JsonSerializerOptions)null),
                json => JsonSerializer.Deserialize<Dictionary<string, object>>(json, (JsonSerializerOptions)null)
            );

            modelBuilder.Entity<DeviceData>()
                .Property(e => e.Data)
                .HasConversion(converter)
                .HasColumnType("jsonb");

            modelBuilder.Entity<DeviceConfig>()
                .Property(e => e.Config)
                .HasConversion(converter)
                .HasColumnType("jsonb");
        }
    }
}
