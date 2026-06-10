using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.Helpers;

public static class JobFactory
{
    public static Job CreatePending(string key = "key-test") =>
        Job.Create("email.send", Priority.High, "{}", key);

    public static Job CreateRunning(string key = "key-test")
    {
        var job = CreatePending(key);
        job.MarkAsRunning();
        return job;
    }
}
