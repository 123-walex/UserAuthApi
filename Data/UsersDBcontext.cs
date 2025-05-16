using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User_Authapi.DTO_s;
using User_Authapi.Data;
using User_Authapi.Entities;

namespace User_Authapi.Data
    {
        public class UsersDbcontext(DbContextOptions<UsersDbcontext> options) : DbContext(options)
        {
            public DbSet<Person> Users => Set<Person>();
            public DbSet<UserSessions> UserSessions => Set<UserSessions>();
            public DbSet<RefreshTokens> RefreshTokens { get; set; } = null!;

        // this customizes how my entities maps to the database
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

             //configure the id as the identity column 
            modelBuilder.Entity<Person>()
                         .Property(p => p.Id)
                            .ValueGeneratedOnAdd();

            modelBuilder.Entity<Person>()
                    .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<UserSessions>()
                       .HasKey(us => us.Id);

            modelBuilder.Entity<UserSessions>()
                .HasOne(us => us.User)
                .WithMany(p => p.Sessions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            var hashedPassword1 = "AQAAAAIAAYagAAAAELZsXLwYZc74iPl1YpZq9E31HbU1M8HBIUX1p5rfeM2MbYxq+vJXqXfsBQcOq1bYlw==";
            var hashedPassword2 = "AQAAAAIAAYagAAAAEM1eECxq3JHBoRxHJdSe+LRfkpRdn0+wxlmAGXxjP6u4bqpN28TmU7DD2chPb6heqA==";
            var hashedPassword3 = "AQAAAAIAAYagAAAAENcXqWZsR2qRhf1O5H1jdfbwPQkY2u7S93z5WMr9ixgKeB3l0KkJ4Xb3KmUjDLQNDg==";

            // showing the data to the database
            modelBuilder.Entity<Person>().HasData
               (
                new Person
                {
                    Id = -1 ,
                    UserName = "wale fagbemi",
                    Password = hashedPassword1,
                    Email = "wale@gmail.com",
                    CreatedAt = SeedConstants.createdAt 
                },
                new Person
                {
                    Id = -2 ,
                    UserName = "moji fagbemi",
                    Password = hashedPassword2,
                    Email = "moji@gmail.com",
                    CreatedAt = SeedConstants.createdAt
                },
                new Person
                {
                    Id = -3 , 
                    UserName = "elizabeth fagbemi",
                    Password = hashedPassword3 ,
                    Email = "elizabeth@gmail.com",
                    CreatedAt = SeedConstants.createdAt
                }
             );
        }
    }
}
