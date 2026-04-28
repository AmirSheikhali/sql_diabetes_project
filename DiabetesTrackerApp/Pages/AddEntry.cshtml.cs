using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

public class AddEntryModel : PageModel
{
    private readonly IConfiguration _config;

    public AddEntryModel(IConfiguration config)
    {
        _config = config;
    }

    [BindProperty]
    public int BG { get; set; }

    [BindProperty]
    public int Carbs { get; set; }

    [BindProperty]
    public double Insulin { get; set; }

    public IActionResult OnPost()
    {
        string connStr = _config.GetConnectionString("DefaultConnection");

        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            conn.Open();

            // Save Blood Glucose
            string bgQuery = @"
                INSERT INTO BloodGlucose (user_id, bg_level, bg_time, bg_status, day_id)
                VALUES (1, @bg, NOW(), 'normal', 1);
            ";

            MySqlCommand bgCmd = new MySqlCommand(bgQuery, conn);
            bgCmd.Parameters.AddWithValue("@bg", BG);
            bgCmd.ExecuteNonQuery();

            // Save Carbs
            string carbQuery = @"
                INSERT INTO CarbIntake (user_id, carbs, timestamp, meal_id, day_id)
                VALUES (1, @carbs, NOW(), 1, 1);
            ";

            MySqlCommand carbCmd = new MySqlCommand(carbQuery, conn);
            carbCmd.Parameters.AddWithValue("@carbs", Carbs);
            carbCmd.ExecuteNonQuery();

            // Save Insulin
            string insulinQuery = @"
                INSERT INTO Insulin (user_id, bg_id, dose, timestamp)
                VALUES (1, LAST_INSERT_ID(), @insulin, NOW());
            ";

            MySqlCommand insulinCmd = new MySqlCommand(insulinQuery, conn);
            insulinCmd.Parameters.AddWithValue("@insulin", Insulin);
            insulinCmd.ExecuteNonQuery();
        }

        return RedirectToPage("/Index");
    }
}