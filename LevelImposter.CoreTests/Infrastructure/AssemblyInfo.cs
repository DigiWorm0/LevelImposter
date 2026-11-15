using Xunit.Sdk;
using Xunit.v3;

// The suite shares static state (translation cache, logger subscribers, Verify
// settings) and is small enough that parallel test execution only adds flakiness.
[assembly: Parallelization(Mode = ParallelMode.None)]
