using System.Collections.Generic;

namespace LocalManipulator.Models
{
    internal abstract class GetTasksResult
    {
        public IEnumerable<TaskForRobot> Tasks { get; set; }
    }

    internal class TaskForRobot
    {
        public string Id { get; set; }
        public string Condition { get; set; }
        public string Codetoexecute { get; set; }
        public string Result { get; set; }
    }
}