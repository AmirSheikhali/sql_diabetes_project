using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

public class IndexModel : PageModel
{
    private readonly IConfiguration _config;

    public List<Entry> Entries { get; set; } = new List<Entry>();

    public List<int> BGValues { get; set; } = new List<int>();
    public List<double> InsulinValues { get; set; } = new List<double>();
    public List<string> Labels { get; set; } = new List<string>();

    public double AvgBG { get; set; }
    public double AvgInsulin { get; set; }

    public int SelectedUser { get; set; }
    public string UserName { get; set; } = "";

    public IndexModel(IConfiguration config)
    {
        _config = config;
    }

    public void OnGet(int? userId)
    {
        SelectedUser = userId ?? 1;

        UserName = SelectedUser switch
        {
            1 => "Amir",
            2 => "David",
            3 => "Jimmy",
            _ => "User"
        };

        string connStr = _config.GetConnectionString("DefaultConnection");

        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();

            string query = @"
            SELECT bg.bg_time, bg.bg_level, bg.note, ins.dose
            FROM BloodGlucose bg
            LEFT JOIN Insulin ins ON bg.bg_id = ins.bg_id
            WHERE bg.user_id = @userId
            ORDER BY bg.bg_time ASC
            LIMIT 7;
            ";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userId", SelectedUser);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TimeSpan time = (TimeSpan)reader["bg_time"];
                        DateTime displayTime = DateTime.Today.Add(time);

                        int bgLevel = Convert.ToInt32(reader["bg_level"]);
                        double insulin = reader["dose"] == DBNull.Value ? 0 : Convert.ToDouble(reader["dose"]);

                        string note = reader["note"]?.ToString() ?? "";

                        Entries.Add(new Entry
                        {
                            Date = displayTime.ToString("hh:mm tt"),
                            BG = bgLevel,
                            Insulin = Math.Round(insulin, 1),
                            Note = note
                        });

                        BGValues.Add(bgLevel);
                        InsulinValues.Add(Math.Round(insulin, 1));
                        Labels.Add(displayTime.ToString("hh:mm tt"));
                    }
                }
            }
        }

        if (BGValues.Count > 0)
            AvgBG = Math.Round(BGValues.Average());

        if (InsulinValues.Count > 0)
            AvgInsulin = Math.Round(InsulinValues.Average(), 1);
    }
}

public class Entry
{
    public string Date { get; set; } = "";
    public int BG { get; set; }
    public double Insulin { get; set; }
    public string Note { get; set; } = "";
}