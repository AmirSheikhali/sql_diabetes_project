using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

public class ReportsModel : PageModel
{
    private readonly IConfiguration _config;

    public List<int> BGValues { get; set; } = new List<int>();
    public List<string> Labels { get; set; } = new List<string>();

    public double AvgBG { get; set; }

    public ReportsModel(IConfiguration config)
    {
        _config = config;
    }

    public void OnGet()
    {
        int selectedUser = 1;

        if (Request.Query.ContainsKey("userId"))
        {
            selectedUser = int.Parse(Request.Query["userId"]);
        }

        string connStr = _config.GetConnectionString("DefaultConnection");

        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();

            string query = @"
            SELECT bg.bg_time, bg.bg_level
            FROM BloodGlucose bg
            WHERE bg.user_id = @userId
            ORDER BY bg.bg_time ASC
            LIMIT 7;
            ";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userId", selectedUser);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // ✅ FIXED TIME HANDLING (THIS WAS THE BUG)
                        TimeSpan time = (TimeSpan)reader["bg_time"];
                        DateTime displayTime = DateTime.Today.Add(time);

                        int bgLevel = Convert.ToInt32(reader["bg_level"]);

                        BGValues.Add(bgLevel);
                        Labels.Add(displayTime.ToString("hh:mm tt"));
                    }
                }
            }
        }

        // ✅ Clean average (no decimals)
        if (BGValues.Count > 0)
        {
            AvgBG = Math.Round(BGValues.Average());
        }
    }
}