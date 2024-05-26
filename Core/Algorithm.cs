using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TimeTable.Core.Models;
using TimeTable.Core.DbContext;

public class TimetableGeneticAlgorithm
{
    private readonly Random _random = new();
    public Semester Semester { get; set; } = null!;

    // Generate the initial population of timetables
    private List<Schedule?> GenerateInitialPopulation(List<Subject> subjects, List<Room> rooms,
        List<Teacher> teachers, List<Student> students, int populationSize)
    {
        var population = new List<Schedule?>();

        for (var i = 0; i < populationSize; i++)
        {
            var timetable = GenerateMaximallyPopulatedTimetable(subjects, rooms, teachers, students);
            population.Add(timetable);
        }

        return population;
    }

    // Generate a single maximally populated timetable
    private Schedule GenerateMaximallyPopulatedTimetable(List<Subject> subjects, List<Room> rooms,
        List<Teacher> teachers, List<Student> students)
    {
        var timetable = new Schedule { Slots = new List<Slot>() };
        var weekSubjectTracker = new Dictionary<int, HashSet<int>>(); // Week number to subjects scheduled in that week

        subjects = subjects.OrderBy(s => _random.Next()).ToList();
        for (var currentDate = Semester.StartDate;
             currentDate <= Semester.EndDate;
             currentDate = currentDate.AddDays(1))
        {
            var dayOfWeek = currentDate.DayOfWeek;
            if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
            {
                var weekOfYear = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(currentDate,
                    CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                if (!weekSubjectTracker.ContainsKey(weekOfYear))
                    weekSubjectTracker[weekOfYear] = new HashSet<int>();

                foreach (var subject in subjects)
                {
                    foreach (var startTime in subject.Setting.SpecificStartTimes)
                    {
                        var slotStartTime = currentDate.Date.Add(startTime);
                        var slotEndTime = slotStartTime.Add(subject.Setting.Duration);

                        if (subject.Setting.Type == "1-day" &&
                            weekSubjectTracker[weekOfYear].Contains(subject.SubjectId))
                            continue; // Skip if already scheduled this week

                        if (IsCourseDuplicatedInSameWeek(subject, timetable, slotStartTime))
                            continue;

                        if (IsCourseDuplicatedOnSameDay(subject, timetable, slotStartTime))
                            continue;

                        var room = FindAvailableRoom(subject, rooms);
                        var teacher = FindAvailableTeacher(subject, teachers, slotStartTime, timetable);

                        if (room != null && teacher != null &&
                            IsRoomAvailable(room, slotStartTime, subject.Setting.Duration, timetable))
                        {
                            if (IsSlotValid(subject, slotStartTime, slotEndTime, timetable))
                            {
                                var slot = new Slot
                                {
                                    Subject = subject,
                                    Room = room,
                                    Teacher = teacher,
                                    StartTime = slotStartTime,
                                    Duration = subject.Setting.Duration
                                };

                                timetable.Slots.Add(slot);
                                weekSubjectTracker[weekOfYear].Add(subject.SubjectId);
                            }
                        }
                    }
                }
            }

            if (currentDate.DayOfWeek == DayOfWeek.Friday)
                currentDate = currentDate.AddDays(2); // Skip to Monday
        }

        return timetable;
    }

    private bool IsCourseDuplicatedInSameWeek(Subject subject, Schedule timetable, DateTime slotStartTime)
    {
        var weekStart = slotStartTime.AddDays(-(int)slotStartTime.DayOfWeek + (int)DayOfWeek.Monday);
        var weekEnd = weekStart.AddDays(6);

        return timetable.Slots.Any(slot =>
            slot.Subject == subject &&
            slot.StartTime >= weekStart &&
            slot.StartTime <= weekEnd);
    }

    private bool IsCourseDuplicatedOnSameDay(Subject subject, Schedule timetable, DateTime slotStartTime)
    {
        return timetable.Slots.Any(slot =>
            slot.Subject == subject &&
            slot.StartTime.Date == slotStartTime.Date);
    }

    private bool ValidateRoomAvailability(List<Subject> subjects, IReadOnlyCollection<Room> rooms)
    {
        var roomTypeDurations = rooms.GroupBy(r => r.Type)
            .ToDictionary(g => g.Key, g => g.Sum(r => 8 * 5)); // Assume 8 hours per day for 5 days a week

        var subjectRoomRequirements = subjects.GroupBy(s => s.RequiredRoomType)
            .ToDictionary(g => g.Key,
                g => g.Sum(s => s.Setting.SpecificStartTimes.Count * s.Setting.Duration.TotalHours));

        foreach (var requirement in subjectRoomRequirements)
        {
            if (!roomTypeDurations.ContainsKey(requirement.Key) ||
                roomTypeDurations[requirement.Key] < requirement.Value)
            {
                Console.WriteLine($"Not enough rooms available for type {requirement.Key}");
                return false;
            }
        }

        return true;
    }

    private Room FindAvailableRoom(Subject subject, IReadOnlyCollection<Room> rooms)
    {
        return rooms.FirstOrDefault(r =>
            r.Type == subject.RequiredRoomType && r.Capacity >= subject.RequiredCapacity);
    }

    private Teacher FindAvailableTeacher(Subject subject, List<Teacher> teachers, DateTime slotStartTime,
        Schedule timetable)
    {
        return subject.TeacherSubjects.Select(a => a.Teacher).FirstOrDefault(t =>
            IsTeacherAvailable(t, slotStartTime, subject.Setting.Duration, timetable));
    }

    private bool IsRoomAvailable(Room room, DateTime slotStartTime, TimeSpan duration, Schedule timetable)
    {
        var slotEndTime = slotStartTime.Add(duration);

        return !timetable.Slots.Any(slot =>
            slot.Room == room &&
            ((slot.StartTime < slotEndTime && slot.StartTime >= slotStartTime) ||
             (slot.EndTime > slotStartTime && slot.EndTime <= slotEndTime) ||
             (slot.StartTime <= slotStartTime && slot.EndTime >= slotEndTime)));
    }

    private bool IsTeacherAvailable(Teacher teacher, DateTime slotStartTime, TimeSpan duration, Schedule timetable)
    {
        var slotEndTime = slotStartTime.Add(duration);

        return teacher.AvailabilityRules
                   .Where(rule => rule.Day == slotStartTime.DayOfWeek)
                   .All(rule =>
                   {
                       var ruleStartTime = slotStartTime.Date.Add(rule.StartTime);
                       var ruleEndTime = slotStartTime.Date.Add(rule.EndTime);
                       return slotEndTime <= ruleStartTime || slotStartTime >= ruleEndTime;
                   }) &&
               !timetable.Slots.Any(slot =>
                   slot.Teacher == teacher &&
                   ((slot.StartTime < slotEndTime && slot.StartTime >= slotStartTime) ||
                    (slot.EndTime > slotStartTime && slot.EndTime <= slotEndTime) ||
                    (slot.StartTime <= slotStartTime && slot.EndTime >= slotEndTime))) &&
               !timetable.Slots.Any(slot =>
                   slot.Teacher == teacher && slot.StartTime == slotStartTime);
    }

    private bool IsSlotValid(Subject subject, DateTime slotStartTime, DateTime slotEndTime, Schedule timetable)
    {
        if (slotStartTime.DayOfWeek == DayOfWeek.Friday && slotEndTime.TimeOfDay > new TimeSpan(12, 0, 0))
        {
            return false;
        }

        if (HasConflicts(timetable, new Slot
            {
                Subject = subject,
                StartTime = slotStartTime,
                Duration = subject.Setting.Duration
            }))
        {
            return false;
        }

        return true;
    }

    private bool HasConflicts(Schedule? timetable, Slot mutatedSlot)
    {
        if (timetable?.Slots == null) return false;
        return timetable.Slots
            .Where(slot => slot != mutatedSlot && slot.StartTime.DayOfWeek == mutatedSlot.StartTime.DayOfWeek).Any(
                slot =>
                    slot.Teacher == mutatedSlot.Teacher && slot.StartTime < mutatedSlot.EndTime &&
                    slot.EndTime > mutatedSlot.StartTime);
    }

    private static void EvaluateFitness(List<Schedule?> population)
    {
        foreach (var timetable in population)
        {
            if (timetable == null) continue;

            var fitness = 0;
            var slotsByDay = timetable.Slots.GroupBy(slot => slot.StartTime.Date);

            foreach (var dayGroup in slotsByDay)
            {
                var slots = dayGroup.ToList();

                // Check for conflicts within the same day
                for (var i = 0; i < slots.Count; i++)
                {
                    for (var j = i + 1; j < slots.Count; j++)
                    {
                        var slotA = slots[i];
                        var slotB = slots[j];

                        // Check for time overlap for the same teacher
                        if (slotA.Teacher == slotB.Teacher && slotA.StartTime < slotB.EndTime &&
                            slotA.EndTime > slotB.StartTime)
                        {
                            fitness -= 10;
                            Console.WriteLine(
                                $"Conflict: Teacher {slotA.Teacher.Name} has overlapping slots on {slotA.StartTime.Date}");
                        }

                        // Check for time overlap for the same room
                        if (slotA.Room == slotB.Room && slotA.StartTime < slotB.EndTime &&
                            slotA.EndTime > slotB.StartTime)
                        {
                            fitness -= 10;
                            Console.WriteLine(
                                $"Conflict: Room {slotA.Room.Name} has overlapping slots on {slotA.StartTime.Date}");
                        }

                        // Check for multiple courses scheduled at the same time
                        if (slotA.StartTime < slotB.EndTime && slotA.EndTime > slotB.StartTime)
                        {
                            fitness -= 5;
                            Console.WriteLine(
                                $"Conflict: Multiple courses are scheduled at the same time on {slotA.StartTime.Date}");
                        }

                        // Check for duplicate subjects on the same day
                        if (slotA.Subject == slotB.Subject)
                        {
                            fitness -= 5;
                            Console.WriteLine(
                                $"Conflict: Subject {slotA.Subject.Name} is duplicated on {slotA.StartTime.Date}");
                        }
                    }
                }
            }

            // Check for duplicate subjects within the same week
            var slotsByWeek = timetable.Slots.GroupBy(slot =>
                CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(slot.StartTime, CalendarWeekRule.FirstDay,
                    DayOfWeek.Monday));

            foreach (var weekGroup in slotsByWeek)
            {
                var slots = weekGroup.ToList();
                var subjectOccurrences = new Dictionary<int, int>();

                foreach (var slot in slots)
                {
                    if (subjectOccurrences.ContainsKey(slot.Subject.SubjectId))
                    {
                        subjectOccurrences[slot.Subject.SubjectId]++;
                    }
                    else
                    {
                        subjectOccurrences[slot.Subject.SubjectId] = 1;
                    }

                    // Penalize for duplicate subjects within the same week
                    if (subjectOccurrences[slot.Subject.SubjectId] > 1)
                    {
                        fitness -= 5;
                        Console.WriteLine(
                            $"Conflict: Subject {slot.Subject.Name} is duplicated in week starting {slot.StartTime.Date}");
                    }
                }
            }

            timetable.Fitness = fitness;
            Console.WriteLine($"Timetable Fitness: {fitness}");
        }
    }

    private void Mutate(Schedule? timetable, IReadOnlyList<Room> rooms, IReadOnlyList<Teacher> teachers,
        List<Student> students)
    {
        if (timetable?.Slots == null || timetable.Slots.Count == 0) return;

        var slotIndex = _random.Next(timetable.Slots.Count);
        var slot = timetable.Slots[slotIndex];

        var originalRoom = slot.Room;
        var originalTeacher = slot.Teacher;
        var originalStartTime = slot.StartTime;

        if (_random.NextDouble() < 0.33)
        {
            Room newRoom;
            do
            {
                newRoom = rooms[_random.Next(rooms.Count)];
            } while (newRoom == slot.Room || !IsRoomAvailable(newRoom, slot.StartTime, slot.Duration, timetable));

            slot.Room = newRoom;
        }
        else if (_random.NextDouble() < 0.66)
        {
            Teacher newTeacher;
            do
            {
                newTeacher = teachers[_random.Next(teachers.Count)];
            } while (slot.Subject.TeacherSubjects.All(ts => ts.TeacherId != newTeacher.TeacherId));

            slot.Teacher = newTeacher;
        }
        else
        {
            var availableTimes = slot.Subject.Setting.SpecificStartTimes;
            var newStartTime = availableTimes[_random.Next(availableTimes.Count)];
            slot.StartTime = slot.StartTime.Date.Add(newStartTime);

            if (IsCourseDuplicatedInSameWeek(slot.Subject, timetable, slot.StartTime) ||
                IsCourseDuplicatedOnSameDay(slot.Subject, timetable, slot.StartTime) ||
                slot.StartTime.DayOfWeek == DayOfWeek.Friday &&
                slot.StartTime.TimeOfDay + slot.Duration > new TimeSpan(12, 0, 0) ||
                timetable.Slots.Any(s =>
                    s.StartTime < slot.EndTime && s.EndTime > slot.StartTime && s.Subject != slot.Subject))
            {
                slot.StartTime = originalStartTime; // Revert to original if duplication or other conflict found
            }
        }

        if (HasConflicts(timetable, slot))
        {
            slot.Room = originalRoom;
            slot.Teacher = originalTeacher;
            slot.StartTime = originalStartTime;
        }
    }

    private Schedule Crossover(Schedule parent1, Schedule parent2, List<Student> students)
    {
        var child = new Schedule { Slots = new List<Slot>() };

        var crossoverPoint = _random.Next(parent1.Slots.Count);

        for (var i = 0; i < crossoverPoint; i++)
        {
            child.Slots.Add(parent1.Slots[i]);
        }

        for (var i = crossoverPoint; i < parent2.Slots.Count; i++)
        {
            var slot = parent2.Slots[i];
            if (!HasConflicts(child, slot) && !child.Slots.Any(s =>
                    s.StartTime < slot.EndTime && s.EndTime > slot.StartTime && s.Subject != slot.Subject))
            {
                child.Slots.Add(slot);
            }
        }

        return child;
    }

    private Schedule SelectParent(IReadOnlyList<Schedule?> population)
    {
        const int tournamentSize = 3;
        var tournament = new List<Schedule?>();

        if (population == null || population.Count == 0)
        {
            throw new ArgumentException("Population cannot be null or empty");
        }

        if (tournamentSize > population.Count)
        {
            throw new ArgumentException("Tournament size must be between 1 and the size of the population");
        }

        for (var i = 0; i < tournamentSize; i++)
        {
            tournament.Add(population[_random.Next(population.Count)]);
        }

        tournament = tournament.Where(t => t != null).ToList();

        if (tournament.Count == 0)
        {
            throw new InvalidOperationException("No valid schedules available for tournament selection");
        }

        return tournament.OrderByDescending(t => t.Fitness).First();
    }

    public Schedule? GeneticAlgorithm(List<Subject> subjects, List<Room> rooms, List<Teacher> teachers,
        List<Student> students, int populationSize, int numGenerations, double mutationRate,
        TimeTableContext dbContext)
    {
        var population = GenerateInitialPopulation(subjects, rooms, teachers, students, populationSize);

        for (var generation = 0; generation < numGenerations; generation++)
        {
            EvaluateFitness(population);

            var newPopulation = new List<Schedule?>();

            while (newPopulation.Count < populationSize)
            {
                var parent1 = SelectParent(population);
                var parent2 = SelectParent(population);

                var child = Crossover(parent1, parent2, students);

                newPopulation.Add(child);
            }

            foreach (var timetable in newPopulation.Where(timetable => _random.NextDouble() < mutationRate))
            {
                Mutate(timetable, rooms, teachers, students);
            }

            population = newPopulation;
        }

        // Final fitness evaluation after all generations
        EvaluateFitness(population);
        var bestTimetable = population.OrderByDescending(t => t.Fitness).First();

        SaveTimetable(bestTimetable, dbContext);

        return PrintTimetable(bestTimetable);
    }

    private void SaveTimetable(Schedule? timetable, TimeTableContext dbContext)
    {
        if (timetable?.Slots == null) return;

        var newTimetable = new TimeTable.Core.Models.TimeTable
        {
            Name = "Generated Timetable",
            Date = DateTime.Now,
            Slots = new List<Slot>(),
            SemesterId = Semester.SemesterId
        };

        foreach (var slot in timetable.Slots)
        {
            Console.WriteLine(
                $"Saving Slot: Subject={slot.Subject.Name}, Teacher={slot.Teacher.Name}, Room={slot.Room.Name}, StartTime={slot.StartTime}, Duration={slot.Duration}");

            newTimetable.Slots.Add(new Slot
            {
                SubjectId = slot.Subject.SubjectId,
                RoomId = slot.Room.RoomId,
                TeacherId = slot.Teacher.TeacherId,
                StartTime = slot.StartTime,
                Duration = slot.Duration
            });
        }

        dbContext.TimeTables.Add(newTimetable);
        dbContext.SaveChanges();
    }

    private static Schedule? PrintTimetable(Schedule? timetable)
    {
        if (timetable?.Slots == null) return timetable;

        Console.WriteLine("Generated Timetable:");

        var sortedSlots = timetable.Slots.OrderBy(slot => slot.StartTime)
            .ToList();

        foreach (var slot in sortedSlots)
        {
            Console.WriteLine(
                $"Subject: {slot.Subject.Name}, Teacher: {slot.Teacher.Name}, Room: {slot.Room.Name}, Start Time: {slot.StartTime}, End Time: {slot.EndTime}");
        }

        return timetable;
    }

    private DayOfWeek? GetOppositeDay(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => DayOfWeek.Wednesday,
            DayOfWeek.Tuesday => DayOfWeek.Thursday,
            DayOfWeek.Wednesday => DayOfWeek.Monday,
            DayOfWeek.Thursday => DayOfWeek.Tuesday,
            _ => (DayOfWeek?)null,
        };
    }
}

public class Schedule
{
    public List<Slot> Slots { get; set; } = new List<Slot>();
    public int Fitness { get; set; }
}