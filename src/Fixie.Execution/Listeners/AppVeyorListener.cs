﻿namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Execution;
    using Newtonsoft.Json;

    public class AppVeyorListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly string url;
        readonly HttpClient client;
        string fileName;

        public AppVeyorListener()
            : this(Environment.GetEnvironmentVariable("APPVEYOR_API_URL"), new HttpClient())
        {
        }

        public AppVeyorListener(string url, HttpClient client)
        {
            this.url = new Uri(new Uri(url), "api/tests").ToString();
            this.client = client;
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Handle(AssemblyStarted message)
        {
            fileName = Path.GetFileName(message.Assembly.Location);
        }

        public void Handle(CaseSkipped message)
        {
            Post(message, x =>
            {
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Post(message);
        }

        public void Handle(CaseFailed message)
        {
            var exception = message.Exception;

            Post(message, x =>
            {
                x.ErrorMessage = exception.Message;
                x.ErrorStackTrace = exception.TypedStackTrace();
            });
        }

        void Post(CaseCompleted message, Action<TestResult> customize = null)
        {
            var testResult = new TestResult
            {
                testFramework = "Fixie",
                fileName = fileName,
                testName = message.Name,
                outcome = message.Status.ToString(),
                durationMilliseconds = message.Duration.TotalMilliseconds.ToString("0"),
                StdOut = message.Output
            };

            customize?.Invoke(testResult);

            Post(testResult);
        }

        void Post(TestResult result)
        {
            var content = JsonConvert.SerializeObject(result);
            client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"))
                  .ContinueWith(x => x.Result.EnsureSuccessStatusCode())
                  .Wait();
        }

        public class TestResult
        {
            public string testFramework { get; set; }
            public string fileName { get; set; }
            public string testName { get; set; }
            public string outcome { get; set; }
            public string durationMilliseconds { get; set; }
            public string StdOut { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorStackTrace { get; set; }
        }
    }
}