using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

public class IndexModel : PageModel
{
    private readonly IConfiguration _config;

    public List<Entry> Entries { get; set; } = new();

    public List<int> BGValues { get; set; } = new();
    public List<int> CarbValues { get; set; } = new();
    public List<double> InsulinValues { get; set; } = new();
    public List<string> Labels { get; set; } = new();

    public int AvgBG { get; set; }
    public int TotalCarbs { get; set; }
    public double TotalInsulin { get; set; }

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

        using var conn = new MySqlConnection(connStr);
        conn.Open();

        string query = @"
        SELECT bg.bg_time, bg.bg_level, bg.note,
               ci.carbs, ins.dose
        FROM BloodGlucose bg
        LEFT JOIN CarbIntake ci 
            ON bg.user_id = ci.user_id AND bg.day_id = ci.day_id
        LEFT JOIN Insulin ins 
            ON bg.bg_id = ins.bg_id
        WHERE bg.user_id = @userId
        ORDER BY bg.bg_time ASC
        LIMIT 7;
        ";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@userId", SelectedUser);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            TimeSpan time = (TimeSpan)reader["bg_time"];
            DateTime displayTime = DateTime.Today.Add(time);

            int bg = Convert.ToInt32(reader["bg_level"]);
            int carbs = reader["carbs"] == DBNull.Value ? 0 : Convert.ToInt32(reader["carbs"]);
            double insulin = reader["dose"] == DBNull.Value ? 0 : Math.Round(Convert.ToDouble(reader["dose"]), 1);

            string note = reader["note"]?.ToString() ?? "";

            Entries.Add(new Entry
            {
                Date = displayTime.ToString("hh:mm tt"),
                BG = bg,
                Carbs = carbs,
                Insulin = insulin,
                Note = note
            });

            Labels.Add(displayTime.ToString("hh:mm tt"));
            BGValues.Add(bg);
            CarbValues.Add(carbs);
            InsulinValues.Add(insulin);
        }

        if (BGValues.Count > 0)
            AvgBG = (int)Math.Round(BGValues.Average());

        TotalCarbs = CarbValues.Sum();
        TotalInsulin = Math.Round(InsulinValues.Sum(), 1);
    }
}

public class Entry
{
    public string Date { get; set; } = "";
    public int BG { get; set; }
    public int Carbs { get; set; }
    public double Insulin { get; set; }
    public string Note { get; set; } = "";
}