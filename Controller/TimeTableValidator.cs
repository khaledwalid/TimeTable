namespace TimeTable.Controller;

using System;
using System.Collections.Generic;
using System.Linq;
using TimeTable.Core.Models;

public class TimetableValidator
{
    public List<string> ValidateConstraints(List<Subject> subjects, List<Room> rooms, List<Teacher> teachers, List<Department> departments, List<College> colleges, Semester semester)
    {
        var errors = new List<string>();

        // 1. Validate subjects
        foreach (var subject in subjects)
        {
            // Check if each subject has assigned teachers
            if (!subject.TeacherSubjects.Any())
            {
                errors.Add($"Subject '{subject.Name}' does not have any assigned teachers.");
            }

            // Check if subject's department is valid
            var department = departments.FirstOrDefault(d => d.DepartmentId == subject.DepartmentId);
            if (department == null)
            {
                errors.Add($"Subject '{subject.Name}' is associated with an invalid department.");
                continue; // Skip further checks for this subject
            }

            // Check if department's college is valid
            var college = colleges.FirstOrDefault(c => c.CollegeId == department.CollegeId);
            if (college == null)
            {
                errors.Add($"Department '{department.Name}' is associated with an invalid college.");
            }

            // Check if subject's setting is valid
            if (subject.Setting == null)
            {
                errors.Add($"Subject '{subject.Name}' does not have a valid setting.");
                continue; // Skip further checks for this subject
            }

            var startTimes = subject.Setting.SpecificStartTimes;
            if (startTimes.Distinct().Count() != startTimes.Count)
            {
                errors.Add($"Subject '{subject.Name}' has duplicate start times in its schedule.");
            }
        }

        // 2. Validate teachers and availability rules
        foreach (var teacher in teachers)
        {
            foreach (var rule in teacher.AvailabilityRules)
            {
                if (rule.StartTime >= rule.EndTime)
                {
                    errors.Add($"Teacher '{teacher.Name}' has an invalid availability rule on {rule.Day} with start time '{rule.StartTime}' and end time '{rule.EndTime}'.");
                }
            }
        }

        // 3. Validate settings
        foreach (var subject in subjects)
        {
            var setting = subject.Setting;
            if (setting.Type != "1-day" && setting.Type != "2-day")
            {
                errors.Add($"Subject '{subject.Name}' has an invalid setting type '{setting.Type}'. It must be either '1-day' or '2-day'.");
            }

            if (setting.SpecificStartTimes == null || !setting.SpecificStartTimes.Any())
            {
                errors.Add($"Subject '{subject.Name}' does not have specific start times defined.");
            }
        }

        // 4. Validate semester dates
        if (semester.StartDate >= semester.EndDate)
        {
            errors.Add($"Semester '{semester.Name}' has an invalid date range from '{semester.StartDate}' to '{semester.EndDate}'.");
        }

        // 5. Validate room availability for subjects
        errors.AddRange(ValidateRoomAvailability(subjects, rooms));

        // 6. Validate colleges
        foreach (var college in colleges)
        {
            if (string.IsNullOrWhiteSpace(college.Name))
            {
                errors.Add($"College with ID '{college.CollegeId}' has an invalid name.");
            }
        }

        // 7. Validate departments
        foreach (var department in departments)
        {
            if (string.IsNullOrWhiteSpace(department.Name))
            {
                errors.Add($"Department with ID '{department.DepartmentId}' has an invalid name.");
            }

            var college = colleges.FirstOrDefault(c => c.CollegeId == department.CollegeId);
            if (college == null)
            {
                errors.Add($"Department '{department.Name}' is associated with an invalid college ID '{department.CollegeId}'.");
            }
        }

        return errors;
    }

    private List<string> ValidateRoomAvailability(List<Subject> subjects, IReadOnlyCollection<Room> rooms)
    {
        var errors = new List<string>();

        var roomTypeDurations = rooms.GroupBy(r => r.Type)
            .ToDictionary(g => g.Key, g => g.Sum(r => 8 * 5)); // Assume 8 hours per day for 5 days a week

        var subjectRoomRequirements = subjects.GroupBy(s => s.RequiredRoomType)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Setting.SpecificStartTimes.Count * s.Setting.Duration.TotalHours));

        foreach (var requirement in subjectRoomRequirements)
        {
            if (!roomTypeDurations.ContainsKey(requirement.Key) || roomTypeDurations[requirement.Key] < requirement.Value)
            {
                errors.Add($"Not enough rooms available for type {requirement.Key}");
            }
        }

        return errors;
    }
}
