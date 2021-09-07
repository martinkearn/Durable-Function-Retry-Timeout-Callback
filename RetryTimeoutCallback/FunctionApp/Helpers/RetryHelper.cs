namespace FunctionApp.Helpers
{
    /// <summary>
    /// Helper methods for retry functionality.
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// Works out if a retry is required.
        /// </summary>
        /// <param name="attempts">The current number of attempts.</param>
        /// <param name="isSuccess">Boolean indicating the isSuccess status of the Attempt.</param>
        /// <param name="maxAttempts">The maximum number of attempts</param>
        /// <returns>Boolean indicating whether to retry or not.</returns>
        public static bool RequireRetry(int attempts, bool isSuccess, int maxAttempts)
        {
            // Overrule other conditions if we are greater than or equal to max attempts count
            if (attempts >= maxAttempts)
            {
                return false;
            }
            else
            {
                // We have not reached maximum attempts so retry based on the opposite of isSuccess (if isSuccess is true then retry is false and vice versa)
                return !isSuccess;
            }
        }
    }
}
