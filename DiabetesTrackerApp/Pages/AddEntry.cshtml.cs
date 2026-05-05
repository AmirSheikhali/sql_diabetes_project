using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;

public class AddEntryModel : PageModel
{
    private readonly IConfiguration _config;

    public AddEntryModel(IConfiguration config)
    {
        _config = config;
    }

    [BindProperty]
    public int UserId { get; set; }

    [BindProperty]
    public int BG { get; set; }

    [BindProperty]
    public int Carbs { get; set; }

    [BindProperty]
    public double Insulin { get; set; }

    [BindProperty]
    public string Note { get; set; } = "";

    public IActionResult OnPost()
    {
        string connStr = _config.GetConnectionString("DefaultConnection");

        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();

            // 1️⃣ Insert Blood Glucose (WITH NOTE)
            string bgQuery = @"
                INSERT INTO BloodGlucose 
                (user_id, bg_level, bg_time, bg_status, day_id, note)
                VALUES (@userId, @bg, NOW(), 'normal', 1, @note);
            ";

            int bgId;

            using (var bgCmd = new MySqlCommand(bgQuery, conn))
            {
                bgCmd.Parameters.AddWithValue("@userId", UserId);
                bgCmd.Parameters.AddWithValue("@bg", BG);
                bgCmd.Parameters.AddWithValue("@note", Note);

                bgCmd.ExecuteNonQuery();

                // Get the ID of the BG we just inserted
                bgId = (int)bgCmd.LastInsertedId;
            }

            // 2️⃣ Insert Carbs
            string carbQuery = @"
                INSERT INTO CarbIntake 
                (user_id, carbs, timestamp, meal_id, day_id)
                VALUES (@userId, @carbs, NOW(), 1, 1);
            ";

            using (var carbCmd = new MySqlCommand(carbQuery, conn))
            {
                carbCmd.Parameters.AddWithValue("@userId", UserId);
                carbCmd.Parameters.AddWithValue("@carbs", Carbs);
                carbCmd.ExecuteNonQuery();
            }

            // 3️⃣ Insert Insulin (linked to BG!)
            string insulinQuery = @"
                INSERT INTO Insulin 
                (user_id, bg_id, dose, timestamp)
                VALUES (@userId, @bgId, @insulin, NOW());
            ";

            using (var insulinCmd = new MySqlCommand(insulinQuery, conn))
            {
                insulinCmd.Parameters.AddWithValue("@userId", UserId);
                insulinCmd.Parameters.AddWithValue("@bgId", bgId);
                insulinCmd.Parameters.AddWithValue("@insulin", Insulin);
                insulinCmd.ExecuteNonQuery();
            }
        }

        // 🔁 Go back to dashboard (with selected user)
        return RedirectToPage("/Index", new { userId = UserId });
    }
}