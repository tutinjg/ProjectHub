using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.Domain.Enums;

public enum TaskItemStatus
{
    Backlog = 1,
    Todo = 2,
    InProgress = 3,
    InReview = 4,
    Done = 5
}