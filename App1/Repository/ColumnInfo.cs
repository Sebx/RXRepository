namespace App1.Repository
{
    using System;
    using System.Collections.Generic;
    using SQLite.Net.Attributes;

    internal class ColumnInfo
    {
        public static IDictionary<string, Type> SQLiteTypeMappings = new Dictionary<string, Type>
        {
            ["integer"] = typeof(int),
            ["bigint"] = typeof(DateTime),
            ["text"] = typeof(string),
            ["varchar"] = typeof(string),
            ["real"] = typeof(double),
            ["double"] = typeof(double),
            ["float"] = typeof(float),
        };

        [Column("cid")]
        public int Cid { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("notnull")]
        public int NotNull { get; set; }

        [Column("pk")]
        public int Pk { get; set; }
    }
}
