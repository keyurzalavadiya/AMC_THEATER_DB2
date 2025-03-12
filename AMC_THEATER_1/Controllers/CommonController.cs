using AMC_THEATER_1.Models;
using IBM.Data.DB2;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AMC_THEATER_1.Controllers
{
    public class CommonController : Controller
    {
         readonly ApplicationDbContext db1 = new ApplicationDbContext(); // Ass
        DB2Connection db = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;");

        public ActionResult Home_Page()
        {
            return View();
        }

        public ActionResult ReceiptFormat()
        {
            return View();
        }
        public ActionResult Seprate_Theater_Tax()
        {
            return View();
        }
        public ActionResult Department_Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Department_Login(LoginViewModel model)
        {
            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                try
                {
                    conn.Open(); // ✅ Open DB2 Connection

                    using (var db = new ApplicationDbContext()) // Use Entity Framework context
                    {
                        // Check if the user exists based on the UN (username/phone number)
                        bool userExists = db.DEPT_LOGIN_DETAILS.Any(u => u.DeptUsername.Trim() == model.UN.Trim());

                        if (!userExists)
                        {
                            ModelState.AddModelError("UN", "User not found.");
                            return View(model);
                        }
                    }

                    return RedirectToAction("DeptHomePage", "Common");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database error: " + ex.Message;
                    return RedirectToAction("Index");
                }
                finally
                {
                    conn.Close(); // ✅ Ensure DB2 connection is closed
                }
            }
        }

        public ActionResult Login()
        {
            Session["HideMenu"] = null;  // Reset sidebar visibility after login
            ViewBag.ShowSideBar = true;  // Ensure sidebar is explicitly shown

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                try
                {
                    conn.Open(); // ✅ Open DB2 Connection

                    using (var db = new ApplicationDbContext()) // Use Entity Framework context
                    {
                        // Ensure phone numbers are compared as strings
                        var userExists = db.USER_LOGIN_DETAILS
                            .Any(u => u.PhoneNumber.ToString() == model.PhoneNumber);

                        if (!userExists)
                        {
                            ModelState.AddModelError("PhoneNumber", "Phone number not found.");
                            return View(model);
                        }
                    }

                    return RedirectToAction("List_of_Application", "Common");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database error: " + ex.Message;
                    return RedirectToAction("Index");
                }
                finally
                {
                    conn.Close(); // ✅ Ensure DB2 connection is closed
                }
            }
        }

        public ActionResult DeptHomePage()
        {


            return View();
        }

        public ActionResult PendingDuesDept()
        {
            // Get current month and year
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                try
                {
                    conn.Open(); // ✅ Open DB2 Connection

                    using (var db = new ApplicationDbContext()) // Use Entity Framework context
                    {
                        // Fetch all approved theaters from DB
                        var theaters = db.TRN_REGISTRATION
                            .Where(tr => tr.TStatus == "Approved" && tr.LicenseDate.HasValue && (tr.TActive.HasValue && tr.TActive.Value == 1)) // ✅ Compare with 1 instead of true
                            .Select(tr => new
                            {
                                tr.TId,
                                tr.TName,
                                tr.TCity,
                                tr.TWard,
                                tr.TZone,
                                tr.TAddress,
                                tr.TTenamentNo,
                                tr.TStatus,
                                tr.LicenseDate
                            })
                            .ToList(); // Execute in memory

                        // Create list to store each month's pending payment status
                        var theaterDueList = new List<TheaterViewModel>();

                        // Loop through each theater and generate month-wise payment status
                        foreach (var theater in theaters)
                        {
                            DateTime startDate = theater.LicenseDate.Value;
                            DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

                            // Ensure startDate is before or equal to currentDate
                            if (startDate > currentDate)
                                continue; // Skip if the LICENSE_DATE is in the future

                            // Generate months from LICENSE_DATE to current month
                            for (DateTime date = startDate; date <= currentDate; date = date.AddMonths(1))
                            {
                                string monthName = date.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture); // Ensure correct case
                                int year = date.Year;

                                bool isPaid = db.THEATER_TAX_PAYMENT
                                    .Any(tp => tp.TId == theater.TId
                                            && tp.PaymentMonth == monthName
                                            && tp.PaymentYear == year);

                                if (!isPaid) // Only add if payment is NOT made
                                {
                                    theaterDueList.Add(new TheaterViewModel
                                    {
                                        T_ID = theater.TId,
                                        T_NAME = theater.TName,
                                        T_CITY = theater.TCity,
                                        T_WARD = theater.TWard,
                                        T_ZONE = theater.TZone,
                                        T_ADDRESS = theater.TAddress,
                                        T_TENAMENT_NO = theater.TTenamentNo,
                                        T_STATUS = theater.TStatus,
                                        SINCE_MONTH = date.ToString("MMMM yyyy"), // Display as "March 2025"
                                        PAYMENT_STATUS = "Not Paid"
                                    });
                                }
                            }
                        }

                        return View(theaterDueList);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database error: " + ex.Message;
                    return RedirectToAction("Index");
                }
                finally
                {
                    conn.Close(); // ✅ Ensure DB2 connection is closed
                }
            }
        }


        [HttpGet]
        public ActionResult PaymentList()
        {
            // Get current month and year
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                try
                {
                    conn.Open(); // ✅ Open DB2 Connection

                    using (var db = new ApplicationDbContext()) // Use Entity Framework context
                    {
                        // Fetch all approved theaters from DB
                        var theaters = db.TRN_REGISTRATION
                            .Where(tr => tr.TStatus == "Approved" && tr.LicenseDate.HasValue) // Ensure LICENSE_DATE exists
                            .Select(tr => new
                            {
                                tr.TId,
                                tr.TName,
                                tr.TCity,
                                tr.TWard,
                                tr.TZone,
                                tr.TAddress,
                                tr.TTenamentNo,
                                tr.TStatus,
                                tr.LicenseDate
                            })
                            .ToList(); // Execute in memory

                        // Create list to store each month's payment status
                        var theaterDueList = new List<TheaterViewModel>();

                        // Loop through each theater and generate month-wise payment status
                        foreach (var theater in theaters)
                        {
                            DateTime startDate = theater.LicenseDate.Value;
                            DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

                            // Generate months from LICENSE_DATE to current month
                            for (DateTime date = startDate; date <= currentDate; date = date.AddMonths(1))
                            {
                                string monthName = date.ToString("MMMM"); // Convert month to string
                                int year = date.Year;

                                bool isPaid = db.THEATER_TAX_PAYMENT
                                    .Any(tp => tp.TId == theater.TId
                                            && tp.PaymentMonth == monthName
                                            && tp.PaymentYear == year);

                                if (isPaid) // Filter to show only Paid records
                                {
                                    theaterDueList.Add(new TheaterViewModel
                                    {
                                        T_ID = theater.TId,
                                        T_NAME = theater.TName,
                                        T_CITY = theater.TCity,
                                        T_WARD = theater.TWard,
                                        T_ZONE = theater.TZone,
                                        T_ADDRESS = theater.TAddress,
                                        T_TENAMENT_NO = theater.TTenamentNo,
                                        T_STATUS = theater.TStatus,
                                        SINCE_MONTH = date.ToString("MMMM yyyy"), // Display as "March 2025"
                                        PAYMENT_STATUS = "Paid"
                                    });
                                }
                            }
                        }

                        // Create filter options for the view
                        ViewBag.StatusFilterOptions = new SelectList(new List<string> { "All", "Paid", "Not Paid Yet" });

                        return View(theaterDueList);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database error: " + ex.Message;
                    return RedirectToAction("Index");
                }
                finally
                {
                    conn.Close(); // ✅ Ensure DB2 connection is closed
                }
            }
        }
        [HttpGet]
        public ActionResult AllReceipt()
        {
            // Get current month and year
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                try
                {
                    conn.Open(); // ✅ Open DB2 Connection

                    using (var db = new ApplicationDbContext()) // Use Entity Framework context
                    {
                        // Fetch all approved theaters from DB
                        var theaters = db.TRN_REGISTRATION
                            .Where(tr => tr.TStatus == "Approved" && tr.LicenseDate.HasValue)
                            .Select(tr => new
                            {
                                tr.TId,
                                tr.TName,
                                tr.TCity,
                                tr.TWard,
                                tr.TZone,
                                tr.TAddress,
                                tr.TTenamentNo,
                                tr.TStatus,
                                tr.LicenseDate
                            })
                            .ToList(); // Execute in memory

                        // Create list to store each month's payment status
                        var theaterDueList = new List<TheaterViewModel>();

                        // Loop through each theater and generate month-wise payment status
                        foreach (var theater in theaters)
                        {
                            DateTime startDate = theater.LicenseDate.Value;
                            DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

                            if (startDate > currentDate)
                                continue; // Skip future start dates

                            // Generate months from LICENSE_DATE to current month
                            for (DateTime date = startDate; date <= currentDate; date = date.AddMonths(1))
                            {
                                string monthName = date.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
                                int year = date.Year;

                                bool isPaid = db.THEATER_TAX_PAYMENT
                                    .Any(tp => tp.TId == theater.TId
                                            && tp.PaymentMonth == monthName
                                            && tp.PaymentYear == year);

                                if (isPaid)
                                {
                                    // Fetch related receipts for this theater
                                    //var receipts = db.RECEIPT_FILTER
                                    //    .Where(r => r.T_ID == theater.TId)
                                    //    .Select(r => new ReceiptFilterViewModel
                                    //    {
                                    //        RCPT_NO = r.RCPT_NO,
                                    //        RCPT_GEN_DATE = r.RCPT_GEN_DATE,
                                    //        T_ID = (int)r.T_ID,
                                    //        T_NAME = theater.TName,
                                    //        PAY_MODE = r.PAY_MODE,
                                    //        STATUS = r.STATUS
                                    //    }).ToList();

                                    theaterDueList.Add(new TheaterViewModel
                                    {
                                        T_ID = theater.TId,
                                        T_NAME = theater.TName,
                                        T_CITY = theater.TCity,
                                        T_WARD = theater.TWard,
                                        T_ZONE = theater.TZone,
                                        T_ADDRESS = theater.TAddress,
                                        T_TENAMENT_NO = theater.TTenamentNo.ToString(),
                                        //STATUS = theater.TStatus,
                                        SINCE_MONTH = date.ToString("MMMM yyyy"), // Display as "March 2025"
                                        PAYMENT_STATUS = "Paid",
                                        //Receipts = receipts // Assign the fetched receipts
                                    });
                                }
                            }
                        }

                        return View(theaterDueList);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database error: " + ex.Message;
                    return RedirectToAction("Index");
                }
                finally
                {
                    conn.Close(); // ✅ Ensure DB2 connection is closed
                }
            }
        }

        //[HttpPost]
        //public ActionResult AllReceipt(ReceiptFilterViewModel model)
        //{
        //    using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
        //    {
        //        try
        //        {
        //            conn.Open(); // ✅ Open DB2 Connection

        //            using (var db = new ApplicationDbContext()) // Use Entity Framework context
        //            {
        //                DateTime? startDate = null;
        //                DateTime? endDate = null;

        //                // Parse From Date
        //                if (!string.IsNullOrEmpty(model.FromDate) && DateTime.TryParse(model.FromDate, out DateTime parsedFromDate))
        //                {
        //                    startDate = parsedFromDate.Date;
        //                }

        //                // Parse To Date
        //                if (!string.IsNullOrEmpty(model.ToDate) && DateTime.TryParse(model.ToDate, out DateTime parsedToDate))
        //                {
        //                    endDate = parsedToDate.Date;
        //                }

        //                // Query to fetch filtered receipt data
        //                var query = db.RECEIPT_FILTER
        //                    .Join(db.TRN_REGISTRATION,
        //                        rf => rf.T_ID,
        //                        tr => tr.TId,
        //                        (rf, tr) => new ReceiptFilterViewModel
        //                        {
        //                            RCPT_NO = rf.RCPT_NO,
        //                            RCPT_GEN_DATE = rf.RCPT_GEN_DATE, // Ensure it's stored as Date
        //                            T_ID = rf.T_ID ?? 0,
        //                            T_NAME = tr.T_NAME,
        //                            PAY_MODE = rf.PAY_MODE,
        //                            STATUS = rf.STATUS
        //                        });

        //                // Apply Filters Dynamically
        //                if (model.TheaterId.HasValue)
        //                {
        //                    query = query.Where(r => r.T_ID == model.TheaterId.Value);
        //                }

        //                if (!string.IsNullOrEmpty(model.StatusFilter))
        //                {
        //                    query = query.Where(r => r.STATUS == model.StatusFilter);
        //                }

        //                if (!string.IsNullOrEmpty(model.PaymentModeFilter))
        //                {
        //                    query = query.Where(r => r.PAY_MODE == model.PaymentModeFilter);
        //                }

        //                // Apply Date Range Filters
        //                if (startDate.HasValue && endDate.HasValue)
        //                {
        //                    query = query.Where(r => r.RCPT_GEN_DATE >= startDate.Value && r.RCPT_GEN_DATE <= endDate.Value);
        //                }
        //                else if (startDate.HasValue)
        //                {
        //                    query = query.Where(r => r.RCPT_GEN_DATE >= startDate.Value);
        //                }
        //                else if (endDate.HasValue)
        //                {
        //                    query = query.Where(r => r.RCPT_GEN_DATE <= endDate.Value);
        //                }

        //                // Execute Query and Return View
        //                var receiptList = query.ToList();
        //                return View(receiptList);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            TempData["Error"] = "Database error: " + ex.Message;
        //            return RedirectToAction("Index");
        //        }
        //        finally
        //        {
        //            conn.Close(); // ✅ Ensure DB2 connection is closed
        //        }
        //    }
        //}
        public ActionResult Dues()
        {
            // Get current month and year
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                try
                {
                    conn.Open(); // ✅ Open DB2 Connection

                    using (var db = new ApplicationDbContext()) // Use Entity Framework context
                    {
                        // Fetch all approved theaters from DB
                        var theaters = db.TRN_REGISTRATION
                            .Where(tr => tr.TStatus == "Approved" && tr.LicenseDate.HasValue && (tr.TActive.HasValue && tr.TActive.Value == 1)) // ✅ Compare with 1 instead of true
                            .Select(tr => new
                            {
                                tr.TId,
                                tr.TName,
                                tr.TCity,
                                tr.TWard,
                                tr.TZone,
                                tr.TAddress,
                                tr.TTenamentNo,
                                tr.TStatus,
                                tr.LicenseDate
                            })
                            .ToList(); // Execute in memory

                        // Create list to store each month's pending payment status
                        var theaterDueList = new List<TheaterViewModel>();

                        // Loop through each theater and generate month-wise payment status
                        foreach (var theater in theaters)
                        {
                            DateTime startDate = theater.LicenseDate.Value;
                            DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

                            // Ensure startDate is before or equal to currentDate
                            if (startDate > currentDate)
                                continue; // Skip if the LICENSE_DATE is in the future

                            // Generate months from LICENSE_DATE to current month
                            for (DateTime date = startDate; date <= currentDate; date = date.AddMonths(1))
                            {
                                string monthName = date.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture); // Ensure correct case
                                int year = date.Year;

                                bool isPaid = db.THEATER_TAX_PAYMENT
                                    .Any(tp => tp.TId == theater.TId
                                            && tp.PaymentMonth == monthName
                                            && tp.PaymentYear == year);

                                if (!isPaid) // Only add if payment is NOT made
                                {
                                    theaterDueList.Add(new TheaterViewModel
                                    {
                                        T_ID = theater.TId,
                                        T_NAME = theater.TName,
                                        T_CITY = theater.TCity,
                                        T_WARD = theater.TWard,
                                        T_ZONE = theater.TZone,
                                        T_ADDRESS = theater.TAddress,
                                        T_TENAMENT_NO = theater.TTenamentNo,
                                        T_STATUS = theater.TStatus,
                                        SINCE_MONTH = date.ToString("MMMM yyyy"), // Display as "March 2025"
                                        PAYMENT_STATUS = "Not Paid"
                                    });
                                }
                            }
                        }

                        return View(theaterDueList);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database error: " + ex.Message;
                    return RedirectToAction("Index");
                }
                finally
                {
                    conn.Close(); // ✅ Ensure DB2 connection is closed
                }
            }
        }

        public ActionResult Theater_Tax()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Theater_Tax(int theater_id)
        {
            if (theater_id <= 0)
            {
                TempData["Error"] = "Please enter a valid Theater ID.";
                return RedirectToAction("Index");
            }

            var model = new TaxPaymentViewModel
            {
                ApplId = theater_id,
                FromMonth = DateTime.Now.ToString("MMMM"), // Default to current month
                ToMonth = DateTime.Now.Year.ToString() // ✅ Convert int to string
            };

            // ✅ Open DB2 Connection
            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                try
                {
                    conn.Open(); // ✅ Ensure connection is opened before querying

                    using (var db = new ApplicationDbContext()) // Use Entity Framework context
                    {
                        // ✅ Step 1: Fetch screen prices first and store in memory
                        var screenPrices = db1.MST_TT_TYPE
                            .AsNoTracking() // Improves performance
                            .ToDictionary(p => p.ScreenType, p => p.ScreenPrice);

                        // ✅ Step 2: Fetch theater details along with associated screens
                        var theaterDetails = db.TRN_REGISTRATION
                            .Where(t => t.ApplId == theater_id)
                            .Select(t => new
                            {
                                TheaterID = t.ApplId,
                                TheaterName = t.TName,
                                MobileNo = t.ManagerContactNo != null ? t.ManagerContactNo.ToString() : string.Empty,
                                Address = t.TAddress,
                                Email = t.TEmail,
                                Screens = db.NO_OF_SCREENS
                                    .Where(s => s.ApplId == t.ApplId)
                                    .ToList() // ✅ Move data to memory first
                            })
                            .FirstOrDefault();

                        if (theaterDetails == null)
                        {
                            TempData["Error"] = "Theater ID not found.";
                            return RedirectToAction("Index");
                        }
                        // ✅ Step 3: Map the screens manually after fetching from DB
                        model.Screens = theaterDetails.Screens
                            .Select(s => new ScreenViewModel
                            {
                                ScreenId = s.ScreenId,
                                AudienceCapacity = (int)s.AudienceCapacity,
                                ScreenType = s.ScreenType,
                                ScreenNo = (int)s.ScreenNo,
                                ScreenPrice = screenPrices.ContainsKey(s.ScreenType)
                                              ? Convert.ToInt32(screenPrices[s.ScreenType]) // Ensure conversion
                                              : 0 // Provide a fallback value
                            }).ToList();

                        model.TheaterName = theaterDetails.TheaterName;
                        model.MobileNo = theaterDetails.MobileNo;
                        model.Address = theaterDetails.Address;
                        model.Email = theaterDetails.Email;
                    }

                    return View(model);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database error: " + ex.Message;
                    return RedirectToAction("Index");
                }
                finally
                {
                    conn.Close(); // ✅ Ensure connection is closed
                }
            }
        }

        [HttpPost]
        public ActionResult ProcessTaxPayment(TaxPaymentViewModel model, HttpPostedFileBase DocumentPath, FormCollection form)
        {
            // ✅ Debug log to check received data
            System.Diagnostics.Debug.WriteLine("Received Model: " + Newtonsoft.Json.JsonConvert.SerializeObject(model));

            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                conn.Open(); // ✅ Open DB2 Connection
                using (var transaction = db1.Database.BeginTransaction())
                {
                    try
                    {
                        string filePath = "Generated Automatically";

                        // ✅ **Step 1: Handle File Upload**
                        if (DocumentPath != null && DocumentPath.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(DocumentPath.FileName);
                            string uploadPath = Path.Combine(Server.MapPath("~/UploadedDoc/"), fileName);

                            if (!Directory.Exists(Server.MapPath("~/UploadedDoc/")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/UploadedDoc/"));
                            }

                            DocumentPath.SaveAs(uploadPath);
                            filePath = "/UploadedDoc/" + fileName;
                        }

                        // ✅ **Step 2: Validate FromMonth & ToMonth**
                        if (string.IsNullOrWhiteSpace(model.FromMonth) || string.IsNullOrWhiteSpace(model.ToMonth))
                        {
                            TempData["Error"] = "From Month and To Month cannot be empty.";
                            return RedirectToAction("Index");
                        }

                        if (!DateTime.TryParseExact(model.FromMonth + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime fromDate) ||
                            !DateTime.TryParseExact(model.ToMonth + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime toDate))
                        {
                            TempData["Error"] = "Invalid date format. Please select valid From and To months.";
                            return RedirectToAction("Index");
                        }

                        // ✅ **Step 3: Ensure Screens List is Populated**
                        if (model.Screens == null || model.Screens.Count == 0)
                        {
                            model.Screens = new List<ScreenViewModel>();

                            int index = 0;
                            while (form[$"Screens[{index}].ScreenType"] != null)
                            {
                                model.Screens.Add(new ScreenViewModel
                                {
                                    ScreenType = form[$"Screens[{index}].ScreenType"],
                                    TotalShow = int.TryParse(form[$"Screens[{index}].TotalShow"], out int totalShow) ? totalShow : 0,
                                    CancelShow = int.TryParse(form[$"Screens[{index}].CancelShow"], out int cancelShow) ? cancelShow : 0,
                                    AmtPerScreen = decimal.TryParse(form[$"Screens[{index}].AmtPerScreen"], out decimal amt) ? amt : 0
                                });

                                index++;
                            }
                        }

                        if (model.Screens.Count == 0)
                        {
                            TempData["Error"] = "No screen data available.";
                            return RedirectToAction("Index");
                        }

                        // ✅ **Step 4: Insert into THEATER_TAX_PAYMENT**
                        for (DateTime currentDate = fromDate; currentDate <= toDate; currentDate = currentDate.AddMonths(1))
                        {
                            var taxPayment = new THEATER_TAX_PAYMENT
                            {
                                ApplId = model.ApplId,
                                PaymentMonth = currentDate.ToString("MMMM"),
                                PaymentYear = currentDate.Year,
                                TaxAmount = model.Screens.Sum(s => s.AmtPerScreen),
                                ShowStatement = filePath,
                                CreateUser = "System",
                                CreateDate = DateTime.Now
                            };

                            db1.THEATER_TAX_PAYMENT.Add(taxPayment);
                            db1.SaveChanges();

                            // ✅ **Step 5: Insert Screens into NO_OF_SCREENS_TAX**
                            foreach (var screen in model.Screens)
                            {
                                var screenTax = new NO_OF_SCREENS_TAX
                                {
                                    ApplId = model.ApplId,
                                    TaxId = taxPayment.TaxId,
                                    ScreenType = screen.ScreenType ?? "Unknown",
                                    TotalShow = screen.TotalShow,
                                    CancelShow = screen.CancelShow,
                                    ActualShow = Math.Max(0, screen.TotalShow - screen.CancelShow),
                                    RatePerScreen = (screen.ScreenType == "Theater") ? 75 : 25,
                                    AmountPerScreen = screen.AmtPerScreen
                                };

                                db1.NO_OF_SCREENS_TAX.Add(screenTax);
                            }
                        }

                        db1.SaveChanges();
                        transaction.Commit();
                        TempData["Success"] = "Tax payment saved successfully!";
                        return RedirectToAction("Theater_Tax");
                    }
                    catch (DbUpdateException ex)
                    {
                        transaction.Rollback();
                        conn.Close();
                        Debug.WriteLine($"DB2 Error: {ex.InnerException?.InnerException?.Message}");
                        TempData["ErrorMessage"] = "Database update error. Check logs for details.";
                        return View(model);
                    }
                    finally
                    {
                        conn.Close(); // ✅ Close DB2 Connection
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult Theater_List()
        {
            ViewBag.CurrentAction = "TheaterList";

            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                try
                {
                    conn.Open(); // ✅ Open DB2 Connection

                    using (var db = new ApplicationDbContext()) // Use Entity Framework context
                    {
                        var query = from tr in db.TRN_REGISTRATION
                                    where tr.TActive.Value == 1 && tr.TStatus == "Approved"
                                    select new
                                    {
                                        //tr.TId,
                                        tr.ApplId,
                                        tr.TName,
                                        tr.TCity,
                                        tr.TAddress,
                                        tr.TTenamentNo,
                                        tr.TZone,
                                        tr.TWard,
                                        tr.TStatus,
                                        tr.RejectReason,
                                        tr.UpdateDate,
                                        tr.TCommencementDate,
                                        //TheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Theater"),
                                        //VideoTheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Video")
                                    };

                        var result = query.ToList();

                        var theaterList = result.Select(tr => new TheaterViewModel
                        {
                           // T_ID = tr.TId,
                            ApplId = tr.ApplId,
                            T_NAME = tr.TName,
                            T_CITY = tr.TCity,
                            T_ADDRESS = tr.TAddress,
                            T_TENAMENT_NO = tr.TTenamentNo,
                            T_ZONE = tr.TZone,
                            T_WARD = tr.TWard,
                            T_STATUS = tr.TStatus,
                            REJECT_REASON = tr.RejectReason,
                            T_COMMENCEMENT_DATE = (DateTime)tr.TCommencementDate,
                            UPDATE_DATE = tr.UpdateDate ?? DateTime.MinValue,
                            //THEATER_SCREEN_COUNT = tr.TheaterScreenCount,
                            //VIDEO_THEATER_SCREEN_COUNT = tr.VideoTheaterScreenCount,
                            //SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoTheaterScreenCount  // Total Screens
                        }).ToList();

                        return View(theaterList);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database error: " + ex.Message;
                    return RedirectToAction("Index");
                }
                finally
                {
                    conn.Close(); // ✅ Ensure DB2 connection is closed
                }
            }
        }

        [HttpPost]
        public ActionResult Theater_List(string theaterId, DateTime? fromDate, DateTime? toDate,
                                         string statusFilter, string cityFilter,
                                         string wardFilter, string zoneFilter,
                                         string theaterTypeFilter, int? deleteId)
        {
            Debug.WriteLine($"Received Data -> FromDate: {fromDate}, ToDate: {toDate}");

            using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
            {
                try
                {
                    conn.Open(); // ✅ Open DB2 Connection

                    using (var db = new ApplicationDbContext()) // Use Entity Framework context
                    {
                        if (deleteId.HasValue)
                        {
                            var theaterToDelete = db.TRN_REGISTRATION.FirstOrDefault(t => t.ApplId == deleteId.Value);
                            if (theaterToDelete != null)
                            {
                                theaterToDelete.TActive = 0;
                                db.SaveChanges();
                            }
                        }

                        var statuses = new List<string> { "Pending", "Approved", "Reject" };
                        var query = db.TRN_REGISTRATION.Where(tr => tr.TActive.HasValue && tr.TActive.Value == 1);


                        if (!string.IsNullOrEmpty(theaterId) && int.TryParse(theaterId, out int theaterIdInt))
                        {
                            query = query.Where(tr => tr.ApplId == theaterIdInt);
                            ViewBag.SelectedTheaterId = theaterId;
                        }

                        if (fromDate.HasValue)
                        {
                            DateTime from = fromDate.Value.Date;
                            query = query.Where(tr => tr.TCommencementDate.HasValue && tr.TCommencementDate.Value >= from);
                        }

                        if (toDate.HasValue)
                        {
                            DateTime to = toDate.Value.Date.AddDays(1).AddTicks(-1);
                            query = query.Where(tr => tr.TCommencementDate.HasValue && tr.TCommencementDate.Value <= to);
                        }

                        if (!string.IsNullOrEmpty(cityFilter))
                        {
                            query = query.Where(tr => tr.TCity == cityFilter);
                            ViewBag.SelectedCity = cityFilter;
                        }

                        if (!string.IsNullOrEmpty(wardFilter))
                        {
                            query = query.Where(tr => tr.TWard == wardFilter);
                            ViewBag.SelectedWard = wardFilter;
                        }

                        if (!string.IsNullOrEmpty(zoneFilter))
                        {
                            query = query.Where(tr => tr.TZone == zoneFilter);
                            ViewBag.SelectedZone = zoneFilter;
                        }

                        if (!string.IsNullOrEmpty(statusFilter) && statuses.Contains(statusFilter))
                        {
                            query = query.Where(tr => tr.TStatus == statusFilter);
                            ViewBag.SelectedStatus = statusFilter;
                        }

                        var result = query.Select(tr => new
                        {
                           // tr.TId,
                            tr.ApplId,
                            tr.TName,
                            tr.TCity,
                            tr.TAddress,
                            tr.TTenamentNo,
                            tr.TZone,
                            tr.TWard,
                            tr.TStatus,
                            //TheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Theater"),
                            //VideoTheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Video")
                        }).ToList();

                        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

                        ViewBag.Cities = result.Select(tr => tr.TCity).Distinct().ToList();
                        ViewBag.Wards = result.Select(tr => tr.TWard).Distinct().ToList();
                        ViewBag.Zones = result.Select(tr => tr.TZone).Distinct().ToList();
                        ViewBag.Statuses = statuses;

                        ViewBag.TheaterTypes = db.MST_TT_TYPE.Select(t => t.ScreenType)
                                                                       .Distinct()
                                                                       .ToList();

                        var theaterList = result.Select(tr => new TheaterViewModel
                        {
                            //T_ID = tr.TId,
                            ApplId = tr.ApplId,  // ✅ Pass REG_ID
                            T_NAME = tr.TName,
                            T_CITY = tr.TCity,
                            T_ADDRESS = tr.TAddress,
                            T_TENAMENT_NO = tr.TTenamentNo.ToString(),
                            T_WARD = tr.TWard,
                            T_ZONE = tr.TZone,
                            T_STATUS = tr.TStatus,
                            //THEATER_SCREEN_COUNT = tr.TheaterScreenCount, // ✅ Theater Screens Count
                            //VIDEO_THEATER_SCREEN_COUNT = tr.VideoTheaterScreenCount, // ✅ Video Screens Count
                            //SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoTheaterScreenCount  // ✅ Total Screens Count
                        }).ToList();

                        return View(theaterList);
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database error: " + ex.Message;
                    return RedirectToAction("Index");
                }
                finally
                {
                    conn.Close(); // ✅ Ensure DB2 connection is closed
                }
            }
        }


        public ActionResult List_of_Application()
        {
            ViewBag.CurrentAction = "List of Application"; // ✅ Important for UI

            List<TheaterViewModel> theaterList = new List<TheaterViewModel>();

            try
            {
                db.Open(); // ✅ Open DB2 connection

                var query = from tr in db1.TRN_REGISTRATION
                            where tr.TActive == 1
                            select new
                            {
                                tr.TId,
                                tr.ApplId,
                                tr.TName,
                                tr.TCity,
                                tr.TAddress,
                                tr.TTenamentNo,
                                tr.TZone,
                                tr.TWard,
                                tr.TStatus,
                                tr.RejectReason,
                                tr.UpdateDate,
                                tr.TCommencementDate,

                                //TheaterScreenCount = (from s in db1.NO_OF_SCREENS
                                //                      where s.ApplId == tr.ApplId && s.ScreenType == "Theater"
                                //                      select s.ScreenId).Count(),
                                //VideoTheaterScreenCount = (from s in db1.NO_OF_SCREENS
                                //                           where s.ApplId == tr.ApplId && s.ScreenType == "Video"
                                //                           select s.ScreenId).Count(),
                            };

                var result = query.ToList();

                theaterList = result.Select(tr => new TheaterViewModel
                {
                    T_ID = tr.TId,
                    ApplId = tr.ApplId,
                    T_NAME = tr.TName,
                    T_CITY = tr.TCity,
                    T_ADDRESS = tr.TAddress,
                    T_TENAMENT_NO = tr.TTenamentNo,
                    T_ZONE = tr.TZone,
                    T_WARD = tr.TWard,
                    T_STATUS = tr.TStatus,
                    REJECT_REASON = tr.RejectReason,
                    T_COMMENCEMENT_DATE = tr.TCommencementDate ?? DateTime.MinValue,
                    UPDATE_DATE = tr.UpdateDate ?? DateTime.MinValue,
                    //SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoTheaterScreenCount,
                    //THEATER_SCREEN_COUNT = tr.TheaterScreenCount,
                    //VIDEO_THEATER_SCREEN_COUNT = tr.VideoTheaterScreenCount
                }).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message; // ✅ Capture DB2 errors
            }
            finally
            {
                db.Close(); // ✅ Ensure DB2 connection is closed
            }

            return View(theaterList);
        }

        public ActionResult ActionRequests()
        {
            ViewBag.CurrentAction = "ActionRequests"; // ✅ Important for UI

            List<TheaterViewModel> theaterList = new List<TheaterViewModel>();

            try
            {
                db.Open(); // ✅ Open DB2 connection

                var query = from tr in db1.TRN_REGISTRATION
                            where tr.TActive == 1
                            select new
                            {
                                tr.TId,
                                tr.ApplId,
                                //tr.RegId,
                                tr.TName,
                                tr.TCity,
                                tr.TAddress,
                                tr.TTenamentNo,
                                tr.TZone,
                                tr.TWard,
                                tr.TStatus,
                                tr.RejectReason,
                                tr.UpdateDate,
                                tr.TCommencementDate,

                                //TheaterScreenCount = (from s in db1.NO_OF_SCREENS
                                //                      where s.ApplId == tr.ApplId && s.ScreenType == "Theater"
                                //                      select s.ScreenId).Count(),
                                //VideoTheaterScreenCount = (from s in db1.NO_OF_SCREENS
                                //                           where s.ApplId == tr.ApplId && s.ScreenType == "Video"
                                //                           select s.ScreenId).Count(),
                            };

                var result = query.ToList();

                theaterList = result.Select(tr => new TheaterViewModel
                {
                    T_ID = tr.TId,
                    ApplId=tr.ApplId,
                    //REG_ID = tr.RegId,
                    T_NAME = tr.TName,
                    T_CITY = tr.TCity,
                    T_ADDRESS = tr.TAddress,
                    T_TENAMENT_NO = tr.TTenamentNo,
                    T_ZONE = tr.TZone,
                    T_WARD = tr.TWard,
                    T_STATUS = tr.TStatus,
                    REJECT_REASON = tr.RejectReason,
                    T_COMMENCEMENT_DATE = tr.TCommencementDate ?? DateTime.MinValue,
                    UPDATE_DATE = tr.UpdateDate ?? DateTime.MinValue,
                    //SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoTheaterScreenCount,
                    //THEATER_SCREEN_COUNT = tr.TheaterScreenCount,
                    //VIDEO_THEATER_SCREEN_COUNT = tr.VideoTheaterScreenCount
                }).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message; // ✅ Capture DB2 errors
            }
            finally
            {
                db.Close(); // ✅ Ensure DB2 connection is closed
            }

            return View(theaterList);
        }
    }
}