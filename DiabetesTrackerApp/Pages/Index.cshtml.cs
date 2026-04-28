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

    public double AvgBG { get; set; }
    public int TotalCarbs { get; set; }
    public double TotalInsulin { get; set; }

    public IndexModel(IConfiguration config)
    {
        _config = config;
    }

    public void OnGet()
    {
        string connStr = _config.GetConnectionString("DefaultConnection");

        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();

            string query = @"
            SELECT d.day_name, bg.bg_level, ci.carbs, ins.dose
            FROM BloodGlucose bg
            JOIN Days d ON bg.day_id = d.day_id
            LEFT JOIN CarbIntake ci 
                ON bg.user_id = ci.user_id AND bg.day_id = ci.day_id
            LEFT JOIN Insulin ins 
                ON bg.bg_id = ins.bg_id
            WHERE bg.user_id = 1
            ORDER BY bg.bg_id DESC
            LIMIT 10;
            ";

            using (var cmd = new MySqlCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Entries.Add(new Entry
                    {
                        Date = reader["day_name"]?.ToString() ?? "",
                        BG = Convert.ToInt32(reader["bg_level"]),
                        Carbs = reader["carbs"] == DBNull.Value ? 0 : Convert.ToInt32(reader["carbs"]),
                        Insulin = reader["dose"] == DBNull.Value ? 0 : Convert.ToDouble(reader["dose"])
                    });
                }
            }
        }

        // ✅ Calculate summary values
        if (Entries.Count > 0)
        {
            AvgBG = Entries.Average(e => e.BG);
            TotalCarbs = Entries.Sum(e => e.Carbs);
            TotalInsulin = Entries.Sum(e => e.Insulin);
        }
    }
}

public class Entry
{
    public string Date { get; set; } = "";
    public int BG { get; set; }
    public int Carbs { get; set; }
    public double Insulin { get; set; }
}