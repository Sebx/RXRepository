namespace App1.Repository
{
    using System;

    public class DbPath
    {
        private readonly string path;

        private DbPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            this.path = path;
        }

        public string Path { get { return this.path; } }

        public static DbPath Create(object value)
        {
            return new DbPath(value.ToString());
        }
    }
}
