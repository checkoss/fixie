﻿namespace Fixie.Execution
{
    using System;
    using System.IO;

    public class ConsoleExecutionEnvironment : IDisposable
    {
        readonly string assemblyFullPath;
        readonly ExecutionProxy executionProxy;

        public ConsoleExecutionEnvironment(string assemblyFullPath)
        {
            this.assemblyFullPath = assemblyFullPath;

            var assemblyDirectory = Path.GetDirectoryName(assemblyFullPath);

            executionProxy = new ExecutionProxy(assemblyDirectory);
        }

        public int RunAssembly(string[] arguments)
            => executionProxy.RunAssembly(assemblyFullPath, arguments);

        public void Dispose()
            => executionProxy.Dispose();
    }
}