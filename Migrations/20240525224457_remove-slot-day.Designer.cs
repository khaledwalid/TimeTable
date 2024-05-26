﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeTable.Core.DbContext;

#nullable disable

namespace TimeTable.Migrations
{
    [DbContext(typeof(TimeTableContext))]
    [Migration("20240525224457_remove-slot-day")]
    partial class removeslotday
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TimeTable.Core.Models.AvailabilityRule", b =>
                {
                    b.Property<int>("AvailabilityRuleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AvailabilityRuleId"));

                    b.Property<int>("Day")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("time");

                    b.Property<int>("TeacherId")
                        .HasColumnType("int");

                    b.HasKey("AvailabilityRuleId");

                    b.HasIndex("TeacherId");

                    b.ToTable("AvailabilityRule");

                    b.HasData(
                        new
                        {
                            AvailabilityRuleId = 1,
                            Day = 1,
                            EndTime = new TimeSpan(0, 10, 0, 0, 0),
                            StartTime = new TimeSpan(0, 8, 0, 0, 0),
                            TeacherId = 1
                        },
                        new
                        {
                            AvailabilityRuleId = 2,
                            Day = 1,
                            EndTime = new TimeSpan(0, 10, 0, 0, 0),
                            StartTime = new TimeSpan(0, 8, 0, 0, 0),
                            TeacherId = 2
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.College", b =>
                {
                    b.Property<int>("CollegeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CollegeId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CollegeId");

                    b.ToTable("Colleges");

                    b.HasData(
                        new
                        {
                            CollegeId = 1,
                            Name = "College of Engineering"
                        },
                        new
                        {
                            CollegeId = 2,
                            Name = "College of Science"
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.Department", b =>
                {
                    b.Property<int>("DepartmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DepartmentId"));

                    b.Property<int>("CollegeId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("DepartmentId");

                    b.HasIndex("CollegeId");

                    b.ToTable("Departments");

                    b.HasData(
                        new
                        {
                            DepartmentId = 1,
                            CollegeId = 1,
                            Name = "Computer Science"
                        },
                        new
                        {
                            DepartmentId = 2,
                            CollegeId = 1,
                            Name = "Electrical Engineering"
                        },
                        new
                        {
                            DepartmentId = 3,
                            CollegeId = 2,
                            Name = "Biology"
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.Room", b =>
                {
                    b.Property<int>("RoomId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RoomId"));

                    b.Property<int>("Capacity")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Zone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RoomId");

                    b.ToTable("Rooms");

                    b.HasData(
                        new
                        {
                            RoomId = 1,
                            Capacity = 100,
                            Name = "Room 101",
                            Type = "Lecture Hall",
                            Zone = "Shared"
                        },
                        new
                        {
                            RoomId = 2,
                            Capacity = 30,
                            Name = "Room 102",
                            Type = "Classroom",
                            Zone = "Male"
                        },
                        new
                        {
                            RoomId = 3,
                            Capacity = 30,
                            Name = "Room 103",
                            Type = "Classroom",
                            Zone = "Female"
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.Semester", b =>
                {
                    b.Property<int>("SemesterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SemesterId"));

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.HasKey("SemesterId");

                    b.ToTable("Semesters");

                    b.HasData(
                        new
                        {
                            SemesterId = 1,
                            EndDate = new DateTime(2024, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Spring 2024",
                            StartDate = new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            SemesterId = 2,
                            EndDate = new DateTime(2024, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Fall 2024",
                            StartDate = new DateTime(2024, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.Setting", b =>
                {
                    b.Property<int>("SettingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SettingId"));

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time");

                    b.Property<string>("SpecificStartTimesJson")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SettingId");

                    b.ToTable("Settings");

                    b.HasData(
                        new
                        {
                            SettingId = 1,
                            Duration = new TimeSpan(0, 1, 15, 0, 0),
                            SpecificStartTimesJson = "{\"1\":[\"08:00:00\",\"09:30:00\",\"11:00:00\",\"12:30:00\",\"14:00:00\",\"15:30:00\",\"17:00:00\"],\"2\":[\"08:00:00\",\"09:30:00\",\"11:00:00\",\"12:30:00\",\"14:00:00\",\"15:30:00\",\"17:00:00\"],\"3\":[\"08:00:00\",\"09:30:00\",\"11:00:00\",\"12:30:00\",\"14:00:00\",\"15:30:00\",\"17:00:00\"],\"4\":[\"08:00:00\",\"09:30:00\",\"11:00:00\",\"12:30:00\",\"14:00:00\",\"15:30:00\",\"17:00:00\"]}"
                        },
                        new
                        {
                            SettingId = 2,
                            Duration = new TimeSpan(0, 2, 15, 0, 0),
                            SpecificStartTimesJson = "{\"1\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"],\"2\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"],\"3\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"],\"4\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"],\"5\":[\"08:00:00\",\"11:00:00\",\"14:00:00\",\"17:00:00\"]}"
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.Slot", b =>
                {
                    b.Property<int>("SlotId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SlotId"));

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time");

                    b.Property<int>("RoomId")
                        .HasColumnType("int");

                    b.Property<int?>("SemesterId")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<int?>("StudentId")
                        .HasColumnType("int");

                    b.Property<int>("SubjectId")
                        .HasColumnType("int");

                    b.Property<int>("TeacherId")
                        .HasColumnType("int");

                    b.Property<int>("TimeTableId")
                        .HasColumnType("int");

                    b.HasKey("SlotId");

                    b.HasIndex("RoomId");

                    b.HasIndex("SemesterId");

                    b.HasIndex("StudentId");

                    b.HasIndex("SubjectId");

                    b.HasIndex("TeacherId");

                    b.HasIndex("TimeTableId");

                    b.ToTable("Slots");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Student", b =>
                {
                    b.Property<int>("StudentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StudentId"));

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StudentId");

                    b.ToTable("Students");

                    b.HasData(
                        new
                        {
                            StudentId = 1,
                            Gender = "Female",
                            Name = "Alice Johnson"
                        },
                        new
                        {
                            StudentId = 2,
                            Gender = "Male",
                            Name = "Bob Smith"
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.Subject", b =>
                {
                    b.Property<int>("SubjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SubjectId"));

                    b.Property<int>("DepartmentId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RequiredCapacity")
                        .HasColumnType("int");

                    b.Property<string>("RequiredRoomType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SettingId")
                        .HasColumnType("int");

                    b.HasKey("SubjectId");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("SettingId");

                    b.ToTable("Subjects");

                    b.HasData(
                        new
                        {
                            SubjectId = 1,
                            DepartmentId = 1,
                            Name = "Introduction to Programming",
                            RequiredCapacity = 50,
                            RequiredRoomType = "Lecture Hall",
                            SettingId = 1
                        },
                        new
                        {
                            SubjectId = 2,
                            DepartmentId = 1,
                            Name = "C++",
                            RequiredCapacity = 50,
                            RequiredRoomType = "Lecture Hall",
                            SettingId = 1
                        },
                        new
                        {
                            SubjectId = 3,
                            DepartmentId = 1,
                            Name = "R+",
                            RequiredCapacity = 50,
                            RequiredRoomType = "Lecture Hall",
                            SettingId = 1
                        },
                        new
                        {
                            SubjectId = 4,
                            DepartmentId = 1,
                            Name = "Android",
                            RequiredCapacity = 50,
                            RequiredRoomType = "Lecture Hall",
                            SettingId = 1
                        },
                        new
                        {
                            SubjectId = 5,
                            DepartmentId = 2,
                            Name = "Circuit Analysis",
                            RequiredCapacity = 20,
                            RequiredRoomType = "Classroom",
                            SettingId = 2
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.Teacher", b =>
                {
                    b.Property<int>("TeacherId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TeacherId"));

                    b.Property<int>("DepartmentId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TeacherId");

                    b.HasIndex("DepartmentId");

                    b.ToTable("Teachers");

                    b.HasData(
                        new
                        {
                            TeacherId = 1,
                            DepartmentId = 1,
                            Name = "John Doe"
                        },
                        new
                        {
                            TeacherId = 2,
                            DepartmentId = 2,
                            Name = "Jane Smith"
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.TeacherSubject", b =>
                {
                    b.Property<int>("TeacherId")
                        .HasColumnType("int");

                    b.Property<int>("SubjectId")
                        .HasColumnType("int");

                    b.HasKey("TeacherId", "SubjectId");

                    b.HasIndex("SubjectId");

                    b.ToTable("TeacherSubject");

                    b.HasData(
                        new
                        {
                            TeacherId = 1,
                            SubjectId = 1
                        },
                        new
                        {
                            TeacherId = 2,
                            SubjectId = 2
                        },
                        new
                        {
                            TeacherId = 2,
                            SubjectId = 3
                        },
                        new
                        {
                            TeacherId = 1,
                            SubjectId = 4
                        },
                        new
                        {
                            TeacherId = 2,
                            SubjectId = 5
                        });
                });

            modelBuilder.Entity("TimeTable.Core.Models.TimeTable", b =>
                {
                    b.Property<int>("TimetableId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TimetableId"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SemesterId")
                        .HasColumnType("int");

                    b.HasKey("TimetableId");

                    b.HasIndex("SemesterId");

                    b.ToTable("TimeTables");
                });

            modelBuilder.Entity("TimeTable.Core.Models.AvailabilityRule", b =>
                {
                    b.HasOne("TimeTable.Core.Models.Teacher", "Teacher")
                        .WithMany("AvailabilityRules")
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Department", b =>
                {
                    b.HasOne("TimeTable.Core.Models.College", "College")
                        .WithMany("Departments")
                        .HasForeignKey("CollegeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("College");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Slot", b =>
                {
                    b.HasOne("TimeTable.Core.Models.Room", "Room")
                        .WithMany("Slots")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TimeTable.Core.Models.Semester", null)
                        .WithMany("Slots")
                        .HasForeignKey("SemesterId");

                    b.HasOne("TimeTable.Core.Models.Student", null)
                        .WithMany("Slots")
                        .HasForeignKey("StudentId");

                    b.HasOne("TimeTable.Core.Models.Subject", "Subject")
                        .WithMany("Slots")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TimeTable.Core.Models.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TimeTable.Core.Models.TimeTable", "TimeTable")
                        .WithMany("Slots")
                        .HasForeignKey("TimeTableId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("Subject");

                    b.Navigation("Teacher");

                    b.Navigation("TimeTable");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Subject", b =>
                {
                    b.HasOne("TimeTable.Core.Models.Department", "Department")
                        .WithMany("Subjects")
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TimeTable.Core.Models.Setting", "Setting")
                        .WithMany()
                        .HasForeignKey("SettingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Department");

                    b.Navigation("Setting");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Teacher", b =>
                {
                    b.HasOne("TimeTable.Core.Models.Department", "Department")
                        .WithMany("Teachers")
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Department");
                });

            modelBuilder.Entity("TimeTable.Core.Models.TeacherSubject", b =>
                {
                    b.HasOne("TimeTable.Core.Models.Subject", "Subject")
                        .WithMany("TeacherSubjects")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TimeTable.Core.Models.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subject");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("TimeTable.Core.Models.TimeTable", b =>
                {
                    b.HasOne("TimeTable.Core.Models.Semester", "Semester")
                        .WithMany("TimeTables")
                        .HasForeignKey("SemesterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Semester");
                });

            modelBuilder.Entity("TimeTable.Core.Models.College", b =>
                {
                    b.Navigation("Departments");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Department", b =>
                {
                    b.Navigation("Subjects");

                    b.Navigation("Teachers");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Room", b =>
                {
                    b.Navigation("Slots");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Semester", b =>
                {
                    b.Navigation("Slots");

                    b.Navigation("TimeTables");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Student", b =>
                {
                    b.Navigation("Slots");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Subject", b =>
                {
                    b.Navigation("Slots");

                    b.Navigation("TeacherSubjects");
                });

            modelBuilder.Entity("TimeTable.Core.Models.Teacher", b =>
                {
                    b.Navigation("AvailabilityRules");
                });

            modelBuilder.Entity("TimeTable.Core.Models.TimeTable", b =>
                {
                    b.Navigation("Slots");
                });
#pragma warning restore 612, 618
        }
    }
}