using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace ATMSimulator.Controllers
{
    public class HomeController : Controller
    {
        private string connStr = "Server=localhost;Database=atm_db;User=root;Password=;";

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ATMCardVerification()
        {
            return View();
        }
        [HttpPost]
        public IActionResult VerifyATMCard(string cardNumber)
        {
            string connStr = "Server=localhost;Database=atm_db;User=root;Password=;";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    Console.WriteLine("Database Connection Opened"); // Debugging log

                    string query = "SELECT * FROM Users WHERE CardNumber = @CardNumber";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                        Console.WriteLine($"Executing Query: {query} with CardNumber: {cardNumber}"); // Debugging log

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // If card number is found
                            {
                                Console.WriteLine("Card Found, Redirecting to EnterPIN");
                                return RedirectToAction("EnterPIN", "Home", new { cardNumber });
                            }
                            else
                            {
                                Console.WriteLine("Invalid Card Number");
                                ViewBag.Error = "Invalid Card Number. Please try again.";
                                return View("ATMCardVerification");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message); // Debugging log
                ViewBag.Error = "An error occurred: " + ex.Message;
                return View("ATMCardVerification");
            }
        }
        public IActionResult EnterPIN(string cardNumber)
        {
            ViewBag.CardNumber = cardNumber;
            return View();
        }
        [HttpPost]
        public IActionResult AuthenticatePIN(string cardNumber, string pin)
        {
            string connStr = "Server=localhost;Database=atm_db;User=root;Password=;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT * FROM Users WHERE CardNumber = @CardNumber AND PIN = @PIN";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                    cmd.Parameters.AddWithValue("@PIN", pin);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // If card and PIN match
                        {
                            return RedirectToAction("MainMenu", "Home", new { cardNumber });
                        }
                        else
                        {
                            ViewBag.Error = "Incorrect PIN. Please try again.";
                            return View("EnterPIN", new { cardNumber });
                        }
                    }
                }
            }
        }
        public IActionResult MainMenu(string cardNumber)
        {
            ViewBag.CardNumber = cardNumber;
            return View();
        }
//CheckBalance
        public IActionResult CheckBalance(string cardNumber)
        {
            string connStr = "Server=localhost;Database=atm_db;User=root;Password=;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    // Step 1: Get UserID from Users Table
                    string getUserQuery = "SELECT UserID FROM Users WHERE CardNumber = @CardNumber";
                    int userId;
                    using (MySqlCommand userCmd = new MySqlCommand(getUserQuery, conn))
                    {
                        userCmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                        object result = userCmd.ExecuteScalar();

                        if (result == null)
                        {
                            ViewBag.Error = "Card number not found!";
                            return View();
                        }

                        userId = Convert.ToInt32(result);
                    }

                    // Step 2: Get Balances for All Accounts
                    string getBalanceQuery = "SELECT AccountType, Balance FROM Accounts WHERE UserID = @UserID";
                    Dictionary<string, decimal> balances = new Dictionary<string, decimal>();

                    using (MySqlCommand balanceCmd = new MySqlCommand(getBalanceQuery, conn))
                    {
                        balanceCmd.Parameters.AddWithValue("@UserID", userId);
                        using (MySqlDataReader reader = balanceCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                balances[reader.GetString(0)] = reader.GetDecimal(1); // AccountType, Balance
                            }
                        }
                    }

                    ViewBag.Balances = balances;
                    ViewBag.CardNumber = cardNumber;
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error fetching balance: " + ex.Message;
                }
            }

            return View();
        }

        //CheckBalance End
        //WithdrawCash
        public IActionResult WithdrawCash(string cardNumber)
        {
            ViewBag.CardNumber = cardNumber;
            return View();
        }
        [HttpPost]
        [HttpPost]
        [HttpPost]
        public IActionResult ProcessWithdrawal(string cardNumber, decimal amount)
        {
            string connStr = "Server=localhost;Database=atm_db;User=root;Password=;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Step 1: Get UserID from Users table
                    string getUserQuery = "SELECT UserID FROM Users WHERE CardNumber = @CardNumber";
                    int userId;
                    using (MySqlCommand userCmd = new MySqlCommand(getUserQuery, conn))
                    {
                        userCmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                        object result = userCmd.ExecuteScalar();

                        if (result == null)
                        {
                            ViewBag.Error = "Card number not found!";
                            ViewBag.CardNumber = cardNumber;
                            return View("WithdrawCash");
                        }

                        userId = Convert.ToInt32(result);
                    }

                    // Step 2: Get AccountID and Check Balance
                    string getAccountQuery = "SELECT AccountID, Balance FROM Accounts WHERE UserID = @UserID";
                    int accountId;
                    decimal balance;
                    using (MySqlCommand accountCmd = new MySqlCommand(getAccountQuery, conn))
                    {
                        accountCmd.Parameters.AddWithValue("@UserID", userId);
                        using (MySqlDataReader reader = accountCmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ViewBag.Error = "No account found for this user!";
                                ViewBag.CardNumber = cardNumber;
                                return View("WithdrawCash");
                            }

                            accountId = reader.GetInt32(0);
                            balance = reader.GetDecimal(1);
                        }
                    }

                    // Step 3: Check if balance is sufficient
                    if (balance < amount)
                    {
                        ViewBag.Error = "Insufficient balance!";
                        ViewBag.CardNumber = cardNumber;
                        return View("WithdrawCash");
                    }

                    // Step 4: Deduct balance in Accounts
                    string updateBalanceQuery = "UPDATE Accounts SET Balance = Balance - @Amount WHERE AccountID = @AccountID";
                    using (MySqlCommand updateCmd = new MySqlCommand(updateBalanceQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@Amount", amount);
                        updateCmd.Parameters.AddWithValue("@AccountID", accountId);
                        updateCmd.ExecuteNonQuery();
                    }

                    // Step 5: Insert transaction record
                    string insertTransactionQuery = "INSERT INTO Transactions (AccountID, CardNumber, TransactionType, Amount, TransactionDate) VALUES (@AccountID, @CardNumber, 'Withdraw', @Amount, NOW())";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertTransactionQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@AccountID", accountId);
                        insertCmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                        insertCmd.Parameters.AddWithValue("@Amount", amount);
                        insertCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return RedirectToAction("WithdrawalSuccess", "Home", new { cardNumber, amount });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ViewBag.Error = "Transaction failed: " + ex.Message;
                    ViewBag.CardNumber = cardNumber;
                    return View("WithdrawCash");
                }
            }
        }



        public IActionResult WithdrawalSuccess(string cardNumber, decimal amount)
        {
            ViewBag.CardNumber = cardNumber;
            ViewBag.Amount = amount;
            return View();
        }
        //WithdrawCash End
        //TransferFunds
        [HttpGet]
        public IActionResult TransferFunds(string cardNumber)
        {
            ViewBag.CardNumber = cardNumber;
            return View();
        }

        [HttpPost]
        public JsonResult ProcessTransfer(string senderCardNumber, string recipientCardNumber, decimal amount)
        {
            string connStr = "Server=localhost;Database=atm_db;User=root;Password=;";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Check sender balance
                    string balanceQuery = "SELECT Balance FROM Accounts WHERE UserID = (SELECT UserID FROM Users WHERE CardNumber = @SenderCard)";
                    using (MySqlCommand balanceCmd = new MySqlCommand(balanceQuery, conn))
                    {
                        balanceCmd.Parameters.AddWithValue("@SenderCard", senderCardNumber);
                        decimal senderBalance = Convert.ToDecimal(balanceCmd.ExecuteScalar());

                        if (senderBalance < amount)
                        {
                            return Json(new { success = false, error = "Insufficient balance!" });
                        }
                    }

                    // Check if recipient exists
                    string recipientQuery = "SELECT UserID FROM Users WHERE CardNumber = @RecipientCard";
                    using (MySqlCommand recipientCmd = new MySqlCommand(recipientQuery, conn))
                    {
                        recipientCmd.Parameters.AddWithValue("@RecipientCard", recipientCardNumber);
                        object recipientId = recipientCmd.ExecuteScalar();

                        if (recipientId == null)
                        {
                            return Json(new { success = false, error = "Recipient card number is invalid!" });
                        }
                    }

                    // Deduct from sender
                    string deductQuery = "UPDATE Accounts SET Balance = Balance - @Amount WHERE UserID = (SELECT UserID FROM Users WHERE CardNumber = @SenderCard)";
                    using (MySqlCommand deductCmd = new MySqlCommand(deductQuery, conn))
                    {
                        deductCmd.Parameters.AddWithValue("@Amount", amount);
                        deductCmd.Parameters.AddWithValue("@SenderCard", senderCardNumber);
                        deductCmd.ExecuteNonQuery();
                    }

                    // Add to recipient
                    string addQuery = "UPDATE Accounts SET Balance = Balance + @Amount WHERE UserID = (SELECT UserID FROM Users WHERE CardNumber = @RecipientCard)";
                    using (MySqlCommand addCmd = new MySqlCommand(addQuery, conn))
                    {
                        addCmd.Parameters.AddWithValue("@Amount", amount);
                        addCmd.Parameters.AddWithValue("@RecipientCard", recipientCardNumber);
                        addCmd.ExecuteNonQuery();
                    }

                    // Insert transaction record
                    string insertQuery = "INSERT INTO Transactions (CardNumber, Type, Amount, Date) VALUES (@SenderCard, 'Transfer Sent', @Amount, NOW()), (@RecipientCard, 'Transfer Received', @Amount, NOW())";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@SenderCard", senderCardNumber);
                        insertCmd.Parameters.AddWithValue("@RecipientCard", recipientCardNumber);
                        insertCmd.Parameters.AddWithValue("@Amount", amount);
                        insertCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return Json(new { success = true, senderCardNumber, recipientCardNumber, amount });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, error = "Transaction failed: " + ex.Message });
                }
            }
        }


        //TransferFunds End
        public IActionResult TestDatabase()
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Users"; // Example query
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Database Connection Failed: " + ex.Message;
                }
            }

            return View(dt);
        }


    }
}
