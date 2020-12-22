namespace Yadex.Retirement.Common
{
    /// <summary>
    /// Class like couple for operation result and message.
    /// </summary>
    public record MsgResult(bool Succeeded, string ErrorMessage)
    {
        /// <summary>
        /// Constructor to represent successful result with zero error message.
        /// </summary>
        public MsgResult() : this(true, "")
        {
        }

        /// <summary>
        /// Constructor to represent failed result with error message.
        /// </summary>
        /// <param name="errorMessage">Error Message</param>
        public MsgResult(string errorMessage) : this(false, errorMessage)
        {
        }
    }
}