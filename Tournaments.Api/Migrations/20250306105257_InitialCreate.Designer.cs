﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tournaments.Api.Data;

#nullable disable

namespace Tournaments.Api.Migrations
{
    [DbContext(typeof(TournamentsApiContext))]
    [Migration("20250306105257_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Tournaments.Shared.Models.Player", b =>
                {
                    b.Property<string>("Gamertag")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Age")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Gamertag");

                    b.ToTable("Player");
                });

            modelBuilder.Entity("Tournaments.Shared.Models.Registration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("PlayerGamertag")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TournamentName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("PlayerGamertag");

                    b.HasIndex("TournamentName");

                    b.ToTable("Registration");
                });

            modelBuilder.Entity("Tournaments.Shared.Models.Tournament", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ParentTournamentName")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Name");

                    b.HasIndex("ParentTournamentName");

                    b.ToTable("Tournament");
                });

            modelBuilder.Entity("Tournaments.Shared.Models.Registration", b =>
                {
                    b.HasOne("Tournaments.Shared.Models.Player", "Player")
                        .WithMany("Registrations")
                        .HasForeignKey("PlayerGamertag")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Tournaments.Shared.Models.Tournament", "Tournament")
                        .WithMany("Registrations")
                        .HasForeignKey("TournamentName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");

                    b.Navigation("Tournament");
                });

            modelBuilder.Entity("Tournaments.Shared.Models.Tournament", b =>
                {
                    b.HasOne("Tournaments.Shared.Models.Tournament", "ParentTournament")
                        .WithMany("SubTournaments")
                        .HasForeignKey("ParentTournamentName");

                    b.Navigation("ParentTournament");
                });

            modelBuilder.Entity("Tournaments.Shared.Models.Player", b =>
                {
                    b.Navigation("Registrations");
                });

            modelBuilder.Entity("Tournaments.Shared.Models.Tournament", b =>
                {
                    b.Navigation("Registrations");

                    b.Navigation("SubTournaments");
                });
#pragma warning restore 612, 618
        }
    }
}
