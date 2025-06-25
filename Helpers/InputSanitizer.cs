using System.Text.RegularExpressions;

namespace MyWebApp.Helpers
{
    public static class InputSanitizer
    {
        /// <summary>
        /// Sanitizes the provided input by removing HTML tags and unwanted characters.
        /// </summary>
        /// <param name="input">The user input to sanitize.</param>
        /// <param name="isEmail">
        /// If true, allows additional characters used in emails; otherwise, restricts to username-friendly characters.
        /// </param>
        /// <returns>A sanitized string with potentially harmful characters removed.</returns>
        public static string Sanitize(string input, bool isEmail = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove HTML tags to prevent XSS.
            string noHtml = Regex.Replace(input, "<.*?>", string.Empty);

            // Determine allowed characters based on input context.
            if (isEmail)
            {
                // For email: allow letters, digits, '@', dot, underscore, and dash.
                noHtml = Regex.Replace(noHtml, @"[^a-zA-Z0-9@._-]", string.Empty);
            }
            else
            {
                // For username: allow letters, digits, underscores, and dashes.
                noHtml = Regex.Replace(noHtml, @"[^a-zA-Z0-9_-]", string.Empty);
            }

            return noHtml;
        }
    }
}
