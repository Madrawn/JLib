using System.Collections;

namespace JLib.DataProvider.Testing;

public abstract class ReflectionTestArguments : IEnumerable<object[]>
{
    /// <summary>
    /// the options to provide to the test method
    /// </summary>
    protected abstract IEnumerable<ReflectionTestOptions> Options { get; }
    /// <summary>
    /// filters for test options with this exact name
    /// </summary>
    protected virtual string Filter { get; } = "";
    /// <summary>
    /// when true, skipped tests will be executed
    /// </summary>
    protected virtual bool ListSkippedTests { get; } = false;

    public IEnumerator<object[]> GetEnumerator()
        => Options
            .Select(options =>
            {
                var skip = !string.IsNullOrWhiteSpace(Filter) && options.TestName != Filter;
                return new object[]
                {
                    options , skip
                };
            })
            //adding the first 2 tests to make sure that even if the test have been filtered, at least one test fails when a filter is applied
            // this guarantees that no tests are skipped unintentionally in the test pipeline
            .Where((parameters, i)
                    => !(bool)parameters[1] || ListSkippedTests
#if RELEASE // guarantees, that when running the test in release mode at least one test fails when a test is focused
                                        || i == 0 || i == 1
#endif
            )
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}