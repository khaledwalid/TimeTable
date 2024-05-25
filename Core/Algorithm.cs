using TimeTable.Core.DbContext;
using TimeTable.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeTable.Core.Dto;

namespace TimeTable.Core
{
    public class TimetableGeneticAlgorithm
    {
        private readonly Random _random = new();
        public Semester Semester { get; set; } = null!;

        private List<Schedule?> GenerateInitialPopulation(List<Subject> subjects, List<Room> rooms,
            List<Teacher> teachers, List<Student> students, int populationSize)
        {
            var population = new List<Schedule?>();

            for (var i = 0; i < populationSize; i++)
            {
                var timetable = GenerateTimetable(subjects, rooms, teachers, students);
                population.Add(timetable);
            }

            return population;
        }

        private bool IsTeacherAvailable(Teacher teacher, DayOfWeek day, DateTime slotStartTime, TimeSpan duration)
        {
            var slotEndTime = slotStartTime.Add(duration);

            foreach (var rule in teacher.AvailabilityRules)
            {
                if (rule.Day == day)
                {
                    var ruleStartTime =
                        new DateTime(slotStartTime.Year, slotStartTime.Month, slotStartTime.Day).Add(rule.StartTime);
                    var ruleEndTime =
                        new DateTime(slotStartTime.Year, slotStartTime.Month, slotStartTime.Day).Add(rule.EndTime);

                    if ((slotStartTime < ruleEndTime && slotStartTime >= ruleStartTime) ||
                        (slotEndTime > ruleStartTime && slotEndTime <= ruleEndTime) ||
                        (slotStartTime <= ruleStartTime && slotEndTime >= ruleEndTime))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsRoomAvailable(Room room, DayOfWeek day, DateTime slotStartTime, TimeSpan duration,
            Schedule timetable)
        {
            var slotEndTime = slotStartTime.Add(duration);

            return !timetable.Slots.Any(slot =>
                slot.Room == room && slot.Day == (int)day &&
                ((slot.StartTime < slotEndTime && slot.StartTime >= slotStartTime) ||
                 (slot.EndTime > slotStartTime && slot.EndTime <= slotEndTime) ||
                 (slot.StartTime <= slotStartTime && slot.EndTime >= slotEndTime)));
        }

        private bool HasCourseConflict(DateTime slotStartTime, DateTime slotEndTime, Schedule timetable)
        {
            return timetable.Slots.Any(slot =>
                (slotStartTime < slot.EndTime && slotStartTime >= slot.StartTime) ||
                (slotEndTime > slot.StartTime && slotEndTime <= slot.EndTime) ||
                (slotStartTime <= slot.StartTime && slotEndTime >= slot.EndTime));
        }

        private Schedule GenerateTimetable(List<Subject> subjects, List<Room> rooms,
            List<Teacher> teachers, List<Student> students)
        {
            var timetable = new Schedule { Slots = new List<Slot>() };

            if (subjects == null || rooms == null || teachers == null || students == null)
            {
                throw new ArgumentException("Subjects, rooms, teachers, and students lists cannot be null");
            }

            ValidateRoomAvailability(subjects, rooms);

            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var subjectSlotCounts = subjects.ToDictionary(s => s.SubjectId, s => 0);

            // Shuffle the subjects to ensure a more random distribution
            subjects = subjects.OrderBy(s => _random.Next()).ToList();

            foreach (var day in Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>())
            {
                if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) continue;

                foreach (var subject in subjects)
                {
                    if (!subject.Setting.SpecificStartTimes.ContainsKey((int)day)) continue;

                    var startTimes = subject.Setting.SpecificStartTimes[(int)day];

                    foreach (var startTime in startTimes)
                    {
                        var room = FindAvailableRoom(subject, rooms);
                        var teacher = FindAvailableTeacher(subject, teachers, day,
                            startOfWeek.AddDays((int)day - 1).Add(startTime), timetable);

                        if (room != null && teacher != null && IsRoomAvailable(room, day,
                                startOfWeek.AddDays((int)day - 1).Add(startTime), subject.Setting.Duration, timetable))
                        {
                            var slotStartTime = startOfWeek.AddDays((int)day - 1).Add(startTime);
                            var slotEndTime = slotStartTime.Add(subject.Setting.Duration);

                            if (IsSlotValid(subject, day, slotStartTime, slotEndTime, timetable))
                            {
                                var slot = new Slot
                                {
                                    Subject = subject,
                                    Room = room,
                                    Teacher = teacher,
                                    Day = (int)day,
                                    StartTime = slotStartTime,
                                    Duration = subject.Setting.Duration
                                };

                                timetable.Slots.Add(slot);
                                subjectSlotCounts[subject.SubjectId]++;
                                LogSlotAdded(subject, day, startTime, teacher, room);
                            }
                        }
                        else
                        {
                            LogNoSuitableRoomOrTeacher(subject, day, startTime, room, teacher);
                        }
                    }
                }
            }

            Console.WriteLine($"Total slots generated: {timetable.Slots.Count}");
            return timetable;
        }

        private bool ValidateRoomAvailability(List<Subject> subjects, IReadOnlyCollection<Room> rooms)
        {
            var roomTypeDurations = rooms.GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.Sum(r => 8 * 5)); // Assume 8 hours per day for 5 days a week

            var subjectRoomRequirements = subjects.GroupBy(s => s.RequiredRoomType)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Setting.SpecificStartTimes.Sum(t => t.Value.Count * s.Setting.Duration.TotalHours)));

            foreach (var requirement in subjectRoomRequirements)
            {
                if (!roomTypeDurations.ContainsKey(requirement.Key) || roomTypeDurations[requirement.Key] < requirement.Value)
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

        private Teacher FindAvailableTeacher(Subject subject, List<Teacher> teachers, DayOfWeek day,
            DateTime slotStartTime, Schedule timetable)
        {
            return subject.TeacherSubjects.Select(a => a.Teacher).FirstOrDefault(t =>
                IsTeacherAvailable(t, day, slotStartTime, subject.Setting.Duration) &&
                !IsTeacherOverlapping(t, day, slotStartTime.TimeOfDay, subject.Setting.Duration, timetable) &&
                !IsTeacherDuplicate(t, day, slotStartTime.TimeOfDay, timetable));
        }

        private bool IsSlotValid(Subject subject, DayOfWeek day, DateTime slotStartTime, DateTime slotEndTime,
            Schedule timetable)
        {
            if (day == DayOfWeek.Friday && slotEndTime.TimeOfDay > new TimeSpan(12, 0, 0))
            {
                Console.WriteLine(
                    $"Skipping slot for subject {subject.Name} on {day} at {slotStartTime.TimeOfDay} because it exceeds allowed time.");
                return false;
            }

            if (HasCourseConflict(slotStartTime, slotEndTime, timetable))
            {
                Console.WriteLine(
                    $"Skipping slot for subject {subject.Name} on {day} at {slotStartTime.TimeOfDay} due to course conflict.");
                return false;
            }

            return true;
        }

        private void LogSlotAdded(Subject subject, DayOfWeek day, TimeSpan startTime, Teacher teacher, Room room)
        {
            Console.WriteLine(
                $"Added slot for subject {subject.Name} on {day} at {startTime} with teacher {teacher.Name} in room {room.Name}.");
        }

        private void LogNoSuitableRoomOrTeacher(Subject subject, DayOfWeek day, TimeSpan startTime, Room room,
            Teacher teacher)
        {
            Console.WriteLine($"No suitable room or teacher found for subject {subject.Name} on {day} at {startTime}");
            if (room == null)
            {
                Console.WriteLine(
                    $"No room found with type {subject.RequiredRoomType} and capacity >= {subject.RequiredCapacity}");
            }

            if (teacher == null)
            {
                Console.WriteLine($"No teacher available for subject {subject.Name} at {startTime} on {day}");
            }
        }

        private bool IsTeacherOverlapping(Teacher teacher, DayOfWeek day, TimeSpan startTime, TimeSpan duration,
            Schedule timetable)
        {
            var slotStartTime = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)day).Add(startTime);
            var slotEndTime = slotStartTime.Add(duration);

            return timetable.Slots.Any(slot =>
                slot.Teacher == teacher && slot.Day == (int)day &&
                ((slot.StartTime < slotEndTime && slot.StartTime >= slotStartTime) ||
                 (slot.EndTime > slotStartTime && slot.EndTime <= slotEndTime) ||
                 (slot.StartTime <= slotStartTime && slot.EndTime >= slotEndTime)));
        }

        private bool IsTeacherDuplicate(Teacher teacher, DayOfWeek day, TimeSpan startTime, Schedule timetable)
        {
            var slotStartTime = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)day).Add(startTime);

            return timetable.Slots.Any(slot =>
                slot.Teacher == teacher && slot.Day == (int)day && slot.StartTime == slotStartTime);
        }

        private static void EvaluateFitness(List<Schedule?> population)
        {
            foreach (var timetable in population)
            {
                var conflicts = 0;

                if (timetable != null)
                {
                    var slotsGroupedByDay = timetable.Slots.GroupBy(slot => slot.Day);

                    foreach (var daySlots in slotsGroupedByDay)
                    {
                        var slotsByTime = daySlots.OrderBy(slot => slot.StartTime).ToList();
                        for (var i = 0; i < slotsByTime.Count - 1; i++)
                        {
                            var currentSlot = slotsByTime[i];

                            for (var j = i + 1; j < slotsByTime.Count; j++)
                            {
                                var nextSlot = slotsByTime[j];

                                if (currentSlot.EndTime <= nextSlot.StartTime) continue;
                                conflicts++;

                                if (currentSlot.Teacher == nextSlot.Teacher)
                                {
                                    conflicts++;

                                    if ((currentSlot.StartTime < nextSlot.EndTime) &&
                                        (currentSlot.EndTime > nextSlot.StartTime))
                                    {
                                        conflicts++;
                                    }

                                    if (currentSlot.Room != nextSlot.Room)
                                    {
                                        conflicts++;
                                    }

                                    if (currentSlot.StartTime == nextSlot.StartTime &&
                                        currentSlot.EndTime == nextSlot.EndTime)
                                    {
                                        conflicts++;
                                    }
                                }

                                if (currentSlot.Room != nextSlot.Room) continue;
                                conflicts++;

                                if (currentSlot.Teacher != nextSlot.Teacher)
                                {
                                    conflicts++;
                                }
                            }
                        }
                    }
                }

                if (timetable != null)
                    timetable.Fitness = -conflicts; // We want to minimize conflicts, so negative conflicts for fitness
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
                } while (newRoom == slot.Room || !IsRoomAvailable(newRoom, (DayOfWeek)slot.Day, slot.StartTime,
                             slot.Duration, timetable));

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
                var availableTimes = slot.Subject.Setting.SpecificStartTimes[slot.Day];
                var newStartTime = availableTimes[_random.Next(availableTimes.Count)];
                slot.StartTime = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + slot.Day).Add(newStartTime);

                if (slot.Day == 5 && slot.StartTime.TimeOfDay + slot.Duration > new TimeSpan(12, 0, 0))
                {
                    slot.StartTime = originalStartTime;
                }
            }

            if (HasConflicts(timetable, slot))
            {
                slot.Room = originalRoom;
                slot.Teacher = originalTeacher;
                slot.StartTime = originalStartTime;
            }
        }

        private static bool HasConflicts(Schedule? timetable, Slot mutatedSlot)
        {
            if (timetable?.Slots == null) return false;
            return timetable.Slots.Where(slot => slot != mutatedSlot && slot.Day == mutatedSlot.Day).Any(slot =>
                slot.Teacher == mutatedSlot.Teacher && slot.StartTime < mutatedSlot.EndTime &&
                slot.EndTime > mutatedSlot.StartTime);
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
                child.Slots.Add(slot);

                if (HasConflicts(child, slot))
                {
                    child.Slots.Remove(slot);
                    break;
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

            EvaluateFitness(population);
            var bestTimetable = population.OrderByDescending(t => t.Fitness).First();

            SaveTimetable(bestTimetable, dbContext);

            return PrintTimetable(bestTimetable);
        }

        private void SaveTimetable(Schedule? timetable, TimeTableContext dbContext)
        {
            if (timetable?.Slots == null) return;

            var newTimetable = new Models.TimeTable
            {
                Name = "Generated Timetable",
                Date = DateTime.Now,
                Slots = new List<Slot>(),
                SemesterId = Semester.SemesterId
            };

            foreach (var slot in timetable.Slots)
            {
                newTimetable.Slots.Add(new Slot
                {
                    SubjectId = slot.Subject.SubjectId,
                    RoomId = slot.Room.RoomId,
                    TeacherId = slot.Teacher.TeacherId,
                    Day = slot.Day,
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

            var sortedSlots = timetable.Slots.OrderBy(slot => slot.Day)
                .ThenBy(slot => slot.StartTime)
                .ToList();

            foreach (var slot in sortedSlots)
            {
                Console.WriteLine(
                    $"Subject: {slot.Subject.Name}, Teacher: {slot.Teacher.Name}, Room: {slot.Room.Name}, Day: {(DayOfWeek)slot.Day}, Start Time: {slot.StartTime.TimeOfDay}, End Time: {slot.EndTime.TimeOfDay}");
            }

            return timetable;
        }
    }
}