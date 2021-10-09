using System.Collections.Generic;

namespace LocalManipulator.Models
{
    public class GetItemsOf<T>
    {
        public IEnumerable<T> Rows { get; set; }
        public int TotalCount { get; set; }
    }

    internal abstract class GetTasksResult
    {
        public IEnumerable<TaskForRobot> Tasks { get; set; }
    }

    public class TaskForRobot
    {
        public string Id { get; set; }
        public string State { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Result { get; set; }
    }
}