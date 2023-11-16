﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RatFriendBackend.Persistence;

#nullable disable

namespace RatFriendBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20231115231729_FriendActivity_AddColumn_ProfileUrl")]
    partial class FriendActivity_AddColumn_ProfileUrl
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("RatFriendBackend.Persistence.Models.FriendActivity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("AppId")
                        .HasColumnType("bigint");

                    b.Property<string>("AppName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("FriendId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("FriendName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ProfileUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("TimePlayed")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("FriendId", "AppId")
                        .IsUnique();

                    b.ToTable("FriendActivities");
                });

            modelBuilder.Entity("RatFriendBackend.Persistence.Models.FriendActivitySubscription", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("FriendActivityId")
                        .HasColumnType("bigint");

                    b.Property<long>("UserSubscriptionId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("FriendActivityId");

                    b.HasIndex("UserSubscriptionId");

                    b.ToTable("FriendActivitySubscriptions");
                });

            modelBuilder.Entity("RatFriendBackend.Persistence.Models.UserSubscription", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<decimal>("FriendId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsFollowing")
                        .HasColumnType("boolean");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<bool>("WantHint")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "FriendId")
                        .IsUnique();

                    b.ToTable("UserSubscription");
                });

            modelBuilder.Entity("RatFriendBackend.Persistence.Models.FriendActivitySubscription", b =>
                {
                    b.HasOne("RatFriendBackend.Persistence.Models.FriendActivity", "FriendActivity")
                        .WithMany("FriendActivitySubscriptions")
                        .HasForeignKey("FriendActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RatFriendBackend.Persistence.Models.UserSubscription", "UserSubscription")
                        .WithMany("FriendActivitySubscriptions")
                        .HasForeignKey("UserSubscriptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FriendActivity");

                    b.Navigation("UserSubscription");
                });

            modelBuilder.Entity("RatFriendBackend.Persistence.Models.FriendActivity", b =>
                {
                    b.Navigation("FriendActivitySubscriptions");
                });

            modelBuilder.Entity("RatFriendBackend.Persistence.Models.UserSubscription", b =>
                {
                    b.Navigation("FriendActivitySubscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
