using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AMC_THEATER_1.Models;
using IBM.Data.DB2;
using Microsoft.Ajax.Utilities;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace Amc_theater.Controllers
{
    public class HomeController : Controller
    {

        //private  THEATER_MODULEEntities1 db = new THEATER_MODULEEntities1();

        //ApplicationDbContext db1 = new ApplicationDbContext(); // Database context
        readonly ApplicationDbContext db = new ApplicationDbContext(); // Ass
        DB2Connection db1 = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;");

        public ActionResult List_of_Application()
        {
            ViewBag.CurrentAction = "List of Application"; // ✅ Important for UI

            List<TheaterViewModel> theaterList = new List<TheaterViewModel>();

            try
            {
                db1.Open(); // ✅ Open DB2 connection

                var query = from tr in db.TRN_REGISTRATION
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
                    ApplId = tr.ApplId,
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
                // 🔴 Log the detailed error, including inner exceptions
                var errorMessage = $"Edit Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner Exception: {ex.InnerException.Message}";
                }

                // Log to a file or database (replace with your logger)
                System.Diagnostics.Debug.WriteLine(errorMessage);

                TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
                return RedirectToAction("List_of_Application", "Home");
            }
            {
                db1.Close(); // ✅ Ensure DB2 connection is closed
            }

            return View(theaterList);
        }

        public ActionResult ActionRequests()
        {
            ViewBag.CurrentAction = "ActionRequests"; // ✅ Important for UI

            List<TheaterViewModel> theaterList = new List<TheaterViewModel>();

            try
            {
                db1.Open(); // ✅ Open DB2 connection

                var query = from tr in db.TRN_REGISTRATION
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
                    ApplId = tr.ApplId,
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
                db1.Close(); // ✅ Ensure DB2 connection is closed
            }

            return View(theaterList);
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
                        var screenPrices = db.MST_TT_TYPE
                            .AsNoTracking() // Improves performance
                            .ToDictionary(p => p.ScreenType, p => p.ScreenPrice);

                        // ✅ Step 2: Fetch theater details along with associated screens
                        var theaterDetails = db.TRN_REGISTRATION
                            .Where(t => t.ApplId == theater_id)
                            .Select(t => new
                            {
                                ApplID = t.ApplId,
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
                //using (var transaction = db1.Database.BeginTransaction())
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

                            db.THEATER_TAX_PAYMENT.Add(taxPayment);
                            db.SaveChanges();

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

                                db.NO_OF_SCREENS_TAX.Add(screenTax);
                            }
                        }

                        db.SaveChanges();
                        //transaction.Commit();
                        TempData["Success"] = "Tax payment saved successfully!";
                        return RedirectToAction("Theater_Tax");
                    }
                    catch (DbUpdateException ex)
                    {
                        //transaction.Rollback();
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



        //public ActionResult Theater_Tax()
        //{
        //    return View();
        //}
        public ActionResult Seprate_Theater_Tax()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Seprate_Theater_Tax(int theater_id)
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
                ToMonth = DateTime.Now.Year.ToString() // Convert int to string
            };
            {
                // Step 1: Fetch screen prices and store them in memory
                var screenPrices = db.MST_TT_TYPE
                    .AsNoTracking()
                    .ToDictionary(p => p.ScreenPrice, p => p.ScreenType);

                // Step 2: Fetch theater details along with associated screens
                var theaterDetails = db.TRN_REGISTRATION
                    .Where(t => t.TId == theater_id.ToString())
                    .Select(t => new
                    {
                        TheaterID = t.TId,
                        TheaterName = t.TName,
                        MobileNo = t.ManagerContactNo != null ? t.ManagerContactNo.ToString() : string.Empty,
                        Address = t.TAddress,
                        Email = t.TEmail,
                        Screens = db.NO_OF_SCREENS
                            .Where(s => s.ApplId.ToString() == t.TId)
                            .ToList()
                    })
                    .FirstOrDefault();

                if (theaterDetails == null)
                {
                    TempData["Error"] = "Theater ID not found.";
                    return RedirectToAction("Index");
                }

                // Step 3: Map the screens manually after fetching from DB
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

        public ActionResult ProcessTaxPaymentSeprate(TaxPaymentViewModel model, HttpPostedFileBase DocumentPath)
        {
            if (model == null || model.Screens == null || model.Screens.Count == 0)
            {
                TempData["Error"] = "Invalid data. Please check your input.";
                return RedirectToAction("Index");
            }
            //using (var transaction = db.Database.BeginTransaction()) // Use EF transaction
            {
                try
                {
                    string filePath = "Generated Automatically"; // Default value

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

                    // ✅ **Step 2: Fetch and Validate FromMonth & ToMonth**
                    if (string.IsNullOrWhiteSpace(model.FromMonth) || string.IsNullOrWhiteSpace(model.ToMonth))
                    {
                        TempData["Error"] = "From Month and To Month cannot be empty.";
                        return RedirectToAction("Index");
                    }

                    DateTime fromDate, toDate;
                    bool isFromValid = DateTime.TryParseExact(model.FromMonth + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out fromDate);
                    bool isToValid = DateTime.TryParseExact(model.ToMonth + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out toDate);

                    if (!isFromValid || !isToValid)
                    {
                        TempData["Error"] = "Invalid date format. Please select valid From and To months.";
                        return RedirectToAction("Index");
                    }

                    // ✅ **Step 3: Insert into THEATER_TAX_PAYMENT (Main Table)**
                    for (DateTime currentDate = fromDate; currentDate <= toDate; currentDate = currentDate.AddMonths(1))
                    {
                        var taxPayment = new THEATER_TAX_PAYMENT
                        {
                            ApplId = model.ApplId,
                            PaymentMonth = currentDate.ToString("MMMM"), // Store the correct month name dynamically
                            PaymentYear = currentDate.Year, // Ensure the correct year is stored
                            TaxAmount = model.Screens.Sum(s => s.AmtPerScreen),
                            ShowStatement = filePath,
                            CreateUser = "System",
                            CreateDate = DateTime.Now
                        };
                        db.THEATER_TAX_PAYMENT.Add(taxPayment);
                        db.SaveChanges();

                        foreach (var screen in model.Screens)
                        {
                            var screenTax = new NO_OF_SCREENS_TAX
                            {
                                ApplId = model.ApplId,
                                TaxId = taxPayment.TaxId,
                                ScreenType = screen.ScreenType,
                                TotalShow = screen.TotalShow,
                                CancelShow = screen.CancelShow,
                                ActualShow = screen.TotalShow - screen.CancelShow,
                                RatePerScreen = (screen.ScreenType == "Theater") ? 75 : 25,
                                AmountPerScreen = screen.AmtPerScreen
                            };
                            db.NO_OF_SCREENS_TAX.Add(screenTax);
                        }
                    }

                    db.SaveChanges();
                    //transaction.Commit();
                    TempData["Success"] = "Tax payment saved successfully!";
                    return RedirectToAction("Theater_Tax");
                }
                catch (Exception ex)
                {
                    //transaction.Rollback();
                    TempData["Error"] = "An error occurred while processing your request: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }

        public ActionResult DeptHomePage()
        {


            return View();
        }



        //public ActionResult List_of_Application()
        //{
        //    ViewBag.CurrentAction = "List of Application"; // This is important!

        //    var query = from tr in db.TRN_REGISTRATION
        //                where (tr.STATUS.Trim().ToLower() == "pending"
        //                    || tr.STATUS.Trim().ToLower() == "rejected"
        //                    || tr.STATUS.Trim().ToLower() == "approved")  // ✅ Include "Approved"
        //                    && tr.T_ACTIVE == true
        //                select new
        //                {
        //                    tr.T_ID,
        //                    tr.REG_ID,
        //                    tr.T_NAME,
        //                    tr.T_CITY,
        //                    tr.T_ADDRESS,
        //                    tr.T_TENAMENT_NO,
        //                    tr.T_ZONE,
        //                    tr.T_WARD,
        //                    tr.STATUS,
        //                    tr.REJECT_REASON,
        //                    tr.UPDATE_DATE,
        //                    //tr.T_OWNER_NAME,
        //                    tr.T_COMMENCEMENT_DATE,

        //                    TheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.T_ID == tr.T_ID && s.SCREEN_TYPE == "Theater"),
        //                    VideoTheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.T_ID == tr.T_ID && s.SCREEN_TYPE == "Video")
        //                };

        //    var result = query.ToList();

        //    var theaterList = result.Select(tr => new TheaterViewModel
        //    {
        //        T_ID = tr.T_ID,
        //        REG_ID = tr.REG_ID,
        //        T_NAME = tr.T_NAME,
        //        T_CITY = tr.T_CITY,
        //        T_ADDRESS = tr.T_ADDRESS,
        //        T_TENAMENT_NO = tr.T_TENAMENT_NO,
        //        T_ZONE = tr.T_ZONE,
        //        T_WARD = tr.T_WARD,
        //        STATUS = tr.STATUS,
        //      //  T_OWNER_NAME = tr.T_OWNER_NAME,
        //        T_COMMENCEMENT_DATE = (DateTime)tr.T_COMMENCEMENT_DATE,
        //        REJECTREASON = tr.REJECT_REASON,
        //        UPDATE_DATE = tr.UPDATE_DATE ?? DateTime.MinValue,
        //        SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoTheaterScreenCount,
        //        THEATER_SCREEN_COUNT = tr.TheaterScreenCount,
        //        VIDEO_THEATER_SCREEN_COUNT = tr.VideoTheaterScreenCount
        //    }).ToList();

        //    return View(theaterList);
        //}
        //public ActionResult List_of_Application()
        //{
        //    ViewBag.CurrentAction = "List of Application"; // This is important!

        //    using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
        //    {
        //        try
        //        {
        //            conn.Open(); // ✅ Open DB2 Connection

        //            using (var db = new ApplicationDbContext()) // Use Entity Framework context
        //            {
        //                var query = from tr in db.TRN_REGISTRATION
        //                            where (tr.TStatus.Trim().ToLower() == "pending"
        //                                || tr.TStatus.Trim().ToLower() == "rejected"
        //                                || tr.TStatus.Trim().ToLower() == "approved")  // ✅ Include "Approved"
        //                                 && (tr.TActive.HasValue && tr.TActive.Value == 1)
        //                            select new
        //                            {
        //                                tr.TId,
        //                                tr.ApplId,
        //                                tr.TName,
        //                                tr.TCity,
        //                                tr.TAddress,
        //                                tr.TTenamentNo,
        //                                tr.TZone,
        //                                tr.TWard,
        //                                tr.TStatus,
        //                                tr.RejectReason,
        //                                tr.UpdateDate,

        //                                TheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Theater"),
        //                                VideoTheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Video")
        //                            };

        //                var result = query.ToList();

        //                var theaterList = result.Select(tr => new TheaterViewModel
        //                {
        //                    T_ID = tr.TId,
        //                    //REG_ID = tr.ApplId.ToString(),
        //                    T_NAME = tr.TName,
        //                    T_CITY = tr.TCity,
        //                    T_ADDRESS = tr.TAddress,
        //                    T_TENAMENT_NO = tr.TTenamentNo,
        //                    T_ZONE = tr.TZone,
        //                    T_WARD = tr.TWard,
        //                    STATUS = tr.TStatus,
        //                    REJECTREASON = tr.RejectReason,
        //                    UPDATE_DATE = tr.UpdateDate ?? DateTime.MinValue,
        //                    SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoTheaterScreenCount,
        //                    THEATER_SCREEN_COUNT = tr.TheaterScreenCount,
        //                    VIDEO_THEATER_SCREEN_COUNT = tr.VideoTheaterScreenCount
        //                }).ToList();

        //                return View(theaterList);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            TempData["Error"] = "Database error: " + ex.Message;
        //            return RedirectToAction("Index");
        //        }
        //        finally
        //        {
        //            conn.Close(); // ✅ Ensure connection is closed
        //        }
        //    }
        //}
        //public ActionResult List_of_Application()
        //{
        //    ViewBag.CurrentAction = "List of Application"; // This is important!

        //    using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
        //    {
        //        try
        //        {
        //            conn.Open(); // ✅ Open DB2 Connection

        //            using (var db = new ApplicationDbContext()) // Use Entity Framework context
        //            {
        //                var query = from tr in db.TRN_REGISTRATION
        //                            where (tr.TStatus.Trim().ToLower() == "pending"
        //                                || tr.TStatus.Trim().ToLower() == "rejected"
        //                                || tr.TStatus.Trim().ToLower() == "approved")
        //                                && (tr.TActive.HasValue && tr.TActive.Value == 1)
        //                            select new
        //                            {
        //                                tr.ApplId,
        //                                tr.TName,
        //                                tr.TCity,
        //                                tr.TAddress,
        //                                tr.TTenamentNo,
        //                                tr.TZone,
        //                                tr.TWard,
        //                                tr.TStatus,
        //                                tr.RejectReason,
        //                                tr.UpdateDate,

        //                                TheaterScreenCount = db.NO_OF_SCREENS.Where(s => s.ApplId == tr.ApplId && s.ScreenType == "Theater").Count(),
        //                                VideoTheaterScreenCount = db.NO_OF_SCREENS.Where(s => s.ApplId == tr.ApplId && s.ScreenType == "Video").Count()
        //                            };


        //                var result = query.ToList();

        //                var theaterList = result.Select(tr => new TheaterViewModel
        //                {
        //                    ApplId = tr.ApplId,
        //                    //REG_ID = tr.ApplId.ToString(),
        //                    T_NAME = tr.TName,
        //                    T_CITY = tr.TCity,
        //                    T_ADDRESS = tr.TAddress,
        //                    T_TENAMENT_NO = tr.TTenamentNo,
        //                    T_ZONE = tr.TZone,
        //                    T_WARD = tr.TWard,
        //                    STATUS = tr.TStatus,
        //                    REJECTREASON = tr.RejectReason,
        //                    UPDATE_DATE = tr.UpdateDate ?? DateTime.MinValue,
        //                    SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoTheaterScreenCount,
        //                    THEATER_SCREEN_COUNT = tr.TheaterScreenCount,
        //                    VIDEO_THEATER_SCREEN_COUNT = tr.VideoTheaterScreenCount
        //                }).ToList();

        //                return View(theaterList);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Exception inner = ex;
        //            while (inner.InnerException != null)
        //            {
        //                inner = inner.InnerException;
        //            }
        //            Console.WriteLine("Error: " + inner.Message);
        //            return RedirectToAction("Index");
        //        }
        //        finally
        //        {
        //            conn.Close(); // ✅ Ensure connection is closed
        //        }
        //    }
        //}



        //// GET: TRN_REGISTRATION
        //public ActionResult Index()
        //{
        //    return View(db.TRN_REGISTRATION.ToList());
        //}

        //// GET: TRN_REGISTRATION/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TRN_REGISTRATION tRN_REGISTRATION = db.TRN_REGISTRATION.Find(id);
        //    if (tRN_REGISTRATION == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tRN_REGISTRATION);
        //}

        // POST: TRN_REGISTRATION/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    TRN_REGISTRATION tRN_REGISTRATION = db.TRN_REGISTRATION.Find(id);
        //    db.TRN_REGISTRATION.Remove(tRN_REGISTRATION);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        // POST: TRN_REGISTRATION/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            {
                TRN_REGISTRATION tRN_REGISTRATION = db.TRN_REGISTRATION.Find(id);
                if (tRN_REGISTRATION == null)
                {
                    TempData["Error"] = "Record not found.";
                    return RedirectToAction("Index");
                }

                db.TRN_REGISTRATION.Remove(tRN_REGISTRATION);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult PendingDuesDept()
        {
            // Get current month and year
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
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

        [HttpGet]
        public ActionResult PaymentList()
        {
            // Get current month and year
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
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

        //[HttpPost]
        //public ActionResult PaymentList(int? theaterId, string fromMonth, string fromYear, string toMonth, string toYear, string statusFilter)
        //{
        //    using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
        //    {
        //        try
        //        {
        //            conn.Open(); // ✅ Open DB2 Connection

        //            using (var db = new ApplicationDbContext()) // Use Entity Framework context
        //            {
        //                DateTime? fromDate = null, toDate = null;
        //                if (!string.IsNullOrEmpty(fromMonth) && !string.IsNullOrEmpty(fromYear))
        //                {
        //                    fromDate = new DateTime(int.Parse(fromYear), int.Parse(fromMonth), 1);
        //                }
        //                if (!string.IsNullOrEmpty(toMonth) && !string.IsNullOrEmpty(toYear))
        //                {
        //                    toDate = new DateTime(int.Parse(toYear), int.Parse(toMonth), DateTime.DaysInMonth(int.Parse(toYear), int.Parse(toMonth)));
        //                }

        //                // Use the correct DbSet for filtering
        //                var payments = db.TRN_REGISTRATION.AsQueryable();

        //                if (theaterId.HasValue)
        //                {
        //                    payments = payments.Where(p => p.TId != null && p.TId.Trim() == theaterId.Value.ToString());

        //                }
        //                if (fromDate.HasValue)
        //                {
        //                    payments = payments.Where(p => p.STARTDATE >= fromDate.Value);
        //                }
        //                if (toDate.HasValue)
        //                {
        //                    payments = payments.Where(p => p.ENDDATE <= toDate.Value);
        //                }
        //                if (!string.IsNullOrEmpty(statusFilter))
        //                {
        //                    payments = payments.Where(p => p.STATUS_PAYMENT == statusFilter);
        //                }

        //                ViewBag.StatusFilterOptions = new SelectList(new List<string> { "Paid", "Pending", "Overdue" });
        //                return View(payments.ToList());
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

            return RedirectToAction("Registration", "Registration");
        }

        public ActionResult Department_Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Department_Login(LoginViewModel model)
        {
            {
                // Check if the user exists based on the UN (username/phone number)
                bool userExists = db.DEPT_LOGIN_DETAILS.Any(u => u.DeptUsername.Trim() == model.UN.Trim());

                if (!userExists)
                {
                    ModelState.AddModelError("UN", "User not found.");
                    return View(model);
                }
            }

            return RedirectToAction("DeptHomePage", "Home");
        }

        [HttpGet]
        public ActionResult Theater_List()
        {
            ViewBag.CurrentAction = "TheaterList";
            {
                var query = from tr in db.TRN_REGISTRATION
                            where tr.TActive.Value == 1 && tr.TStatus == "Approved"
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
                                //TheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Theater"),
                                //VideoTheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Video")
                            };

                var result = query.ToList();

                var theaterList = result.Select(tr => new TheaterViewModel
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
                    T_COMMENCEMENT_DATE = (DateTime)tr.TCommencementDate,
                    UPDATE_DATE = tr.UpdateDate ?? DateTime.MinValue,
                    //THEATER_SCREEN_COUNT = tr.TheaterScreenCount,
                    //VIDEO_THEATER_SCREEN_COUNT = tr.VideoTheaterScreenCount,
                    //SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoTheaterScreenCount  // Total Screens
                }).ToList();

                return View(theaterList);
            }
        }


        [HttpPost]
        public ActionResult Theater_List(string theaterId, DateTime? fromDate, DateTime? toDate,
                                   string statusFilter, string cityFilter,
                                   string wardFilter, string zoneFilter,
                                   string theaterTypeFilter, int? deleteId)
        {
            Debug.WriteLine($"Received Data -> FromDate: {fromDate}, ToDate: {toDate}");
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
                    tr.TId,
                    tr.ApplId,
                    tr.TName,
                    tr.TCity,
                    tr.TAddress,
                    tr.TTenamentNo,
                    tr.TZone,
                    tr.TWard,
                    tr.TStatus,
                    TheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Theater"),
                    VideoTheaterScreenCount = db.NO_OF_SCREENS.Count(s => s.ApplId == tr.ApplId && s.ScreenType == "Video")
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
                    T_ID = tr.TId,
                    ApplId = tr.ApplId,  // ✅ Pass REG_ID
                    T_NAME = tr.TName,
                    T_CITY = tr.TCity,
                    T_ADDRESS = tr.TAddress,
                    T_TENAMENT_NO = tr.TTenamentNo.ToString(),
                    T_WARD = tr.TWard,
                    T_ZONE = tr.TZone,
                    T_STATUS = tr.TStatus,
                    THEATER_SCREEN_COUNT = tr.TheaterScreenCount, // ✅ Theater Screens Count
                    VIDEO_THEATER_SCREEN_COUNT = tr.VideoTheaterScreenCount, // ✅ Video Screens Count
                    SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoTheaterScreenCount  // ✅ Total Screens Count
                }).ToList();

                return View(theaterList);
            }
        }
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
                            .Where(tr => tr.TStatus == "Approved" && tr.TCommencementDate.HasValue && (tr.TActive.HasValue && tr.TActive.Value == 1)) // ✅ Compare with 1 instead of true
                            .Select(tr => new
                            {
                                tr.ApplId,
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
                                    .Any(tp => tp.ApplId == theater.ApplId
                                            && tp.PaymentMonth == monthName
                                            && tp.PaymentYear == year);

                                if (!isPaid) // Only add if payment is NOT made
                                {
                                    theaterDueList.Add(new TheaterViewModel
                                    {
                                        ApplId = theater.ApplId,
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

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
        //public ActionResult Theater_Registration()
        //{

        //    return View();
        //}


        //public ActionResult Theater_Tax(bool? hideMenu)
        //{
        //    if (hideMenu == true)
        //    {
        //        Session["HideMenu"] = true;  // Hide sidebar when clicking "Pay Now"
        //    }
        //    else if (Session["HideMenu"] != null)
        //    {
        //        hideMenu = (bool)Session["HideMenu"];
        //    }
        //    else
        //    {
        //        hideMenu = false; // Default to showing sidebar
        //    }

        //    ViewBag.ShowSideBar = !hideMenu; // Control sidebar visibility

        //    return View();
        //}


        //public ActionResult Make_Payment()
        //{

        //    return View();
        //}



        //public ActionResult Payment_Receipt()
        //{

        //    return View();
        //}
        //public ActionResult FormWithPagination()
        //{
        //    return View();
        //}

        // Handle form submission (if necessary)
        //[HttpPost]
        //public ActionResult SubmitForm(FormCollection form)
        //{
        //    // Access the form data using the FormCollection
        //    var name = form["name"];
        //    var email = form["email"];
        //    var address = form["address"];
        //    var city = form["city"];
        //    var cardNumber = form["cardNumber"];
        //    var expiry = form["expiry"];

        //    // Do something with the form data (e.g., save to database)

        //    return RedirectToAction("Success");
        //}




        [HttpGet]
        public ActionResult Receipt()
        {
            return View();
        }
        public ActionResult Edit_Registration()
        {
            return View();
        }


        public ActionResult Success()
        {
            return View();
        }
        public ActionResult New()
        {
            return View();
        }
        [HttpGet]
        public ActionResult SearchID()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SearchID(string searchTerm)
        {
            {
                var results = db.TRN_REGISTRATION
                    .Where(t => t.TTenamentNo == searchTerm)
                    .Select(t => new SearchViewModel
                    {
                        T_NAME = t.TName,
                        REG_ID = t.ApplId.ToString()
                        // Add other properties if needed
                    })
                    .ToList();

                return View(results);
            }
        }


        //[HttpPost]
        //public ActionResult Theater_Tax(int theater_id)
        //{
        //    if (theater_id <= 0)
        //    {
        //        TempData["Error"] = "Please enter a valid Theater ID.";
        //        return RedirectToAction("Index");
        //    }

        //    // Fetch the theater details along with the associated screens
        //    var theaterDetails = db.TRN_REGISTRATION
        //        .Where(t => t.T_ID == theater_id)
        //        .Select(t => new
        //        {
        //            TheaterID = t.T_ID,
        //            OwnerName = t.T_OWNER_NAME,
        //            MobileNo = t.T_OWNER_NUMBER != null ? t.T_OWNER_NUMBER.ToString() : string.Empty,
        //            Address = t.T_ADDRESS,
        //            Email = t.T_OWNER_EMAIL,
        //            Screens = db.NO_OF_SCREENS
        //                .Where(s => s.T_ID == t.T_ID)
        //                .Select(s => new ScreenViewModel
        //                {
        //                    ScreenId = s.SCREEN_ID,
        //                    AudienceCapacity =(int) s.AUDIENCE_CAPACITY,
        //                    ScreenType = s.SCREEN_TYPE
        //                })
        //                .ToList()
        //        })
        //        .FirstOrDefault();

        //    if (theaterDetails == null)
        //    {
        //        TempData["Error"] = "Theater ID not found.";
        //        return RedirectToAction("Index");
        //    }

        //    // Create a new TaxPayment view model and populate it with theater details and screens
        //    var taxPayment = new TaxPayment
        //    {
        //        TheaterID = theaterDetails.TheaterID,
        //        OwnerName = theaterDetails.OwnerName,
        //        MobileNo = theaterDetails.MobileNo,
        //        Address = theaterDetails.Address,
        //        Email = theaterDetails.Email,
        //        Screens = theaterDetails.Screens // Assuming TaxPayment has a Screens property
        //    };

        //    return View(taxPayment); // Return the same view with the model populated
        //}

        public ActionResult PaymentHistoryFilter()
        {

            return View();
        }

        public ActionResult Home_Page()
        {
            return View();
        }

        // [HttpPost]
        // public ActionResult PaymentHistoryFilter(
        //int? theaterId,
        //string fromDate,
        //string toDate,
        //string statusFilter,
        //decimal? minAmount,
        //decimal? maxAmount)
        // {
        //     // Initialize date variables
        //     DateTime? startDate = null;
        //     DateTime? endDate = null;

        //     if (DateTime.TryParse(fromDate, out DateTime parsedFromDate))
        //     {
        //         startDate = parsedFromDate;
        //     }

        //     if (DateTime.TryParse(toDate, out DateTime parsedToDate))
        //     {
        //         endDate = parsedToDate;
        //     }

        //     var query = db.PAYMENT_HISTORY.AsQueryable();

        //     if (theaterId.HasValue)
        //     {
        //         query = query.Where(p => p.T_ID == theaterId.Value);
        //     }

        //     if (!string.IsNullOrEmpty(statusFilter))
        //     {
        //         query = query.Where(p => p.STATUS == statusFilter);
        //     }

        //     if (startDate.HasValue && endDate.HasValue)
        //     {
        //         query = query.Where(p => p.PAYMENT_DATE >= startDate.Value && p.PAYMENT_DATE <= endDate.Value);
        //     }
        //     else if (startDate.HasValue)
        //     {
        //         query = query.Where(p => p.PAYMENT_DATE >= startDate.Value);
        //     }
        //     else if (endDate.HasValue)
        //     {
        //         query = query.Where(p => p.PAYMENT_DATE <= endDate.Value);
        //     }

        //     if (minAmount.HasValue)
        //     {
        //         query = query.Where(p => p.AMOUNT >= minAmount.Value);
        //     }

        //     if (maxAmount.HasValue)
        //     {
        //         query = query.Where(p => p.AMOUNT <= maxAmount.Value);
        //     }

        //     // Convert to ViewModel and format date
        //     var filteredPayments = query.ToList().Select(p => new PaymentHistoryViewModel
        //     {
        //         PAYMENT_ID = p.PAYMENT_ID,
        //         T_ID = p.T_ID,
        //         AMOUNT = p.AMOUNT,
        //         STATUS = p.STATUS,
        //         PAYMENT_DATE = p.PAYMENT_DATE.ToString("dd-MM-yyyy") // ✅ Correctly formatted date
        //     }).ToList();

        //     return View(filteredPayments);
        // }

        //    [HttpPost]
        //    public ActionResult PaymentHistoryFilter(
        //int? theaterId,
        //string fromDate,
        //string toDate,
        //string statusFilter,
        //decimal? minAmount,
        //decimal? maxAmount)
        //    {
        //        using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
        //        {
        //            try
        //            {
        //                conn.Open(); // ✅ Open DB2 Connection

        //                using (var db = new ApplicationDbContext()) // Use Entity Framework context
        //                {
        //                    // Initialize date variables
        //                    DateTime? startDate = null;
        //                    DateTime? endDate = null;

        //                    if (DateTime.TryParse(fromDate, out DateTime parsedFromDate))
        //                    {
        //                        startDate = parsedFromDate;
        //                    }

        //                    if (DateTime.TryParse(toDate, out DateTime parsedToDate))
        //                    {
        //                        endDate = parsedToDate;
        //                    }

        //                    var query = db.PAYMENT_HISTORY.AsQueryable();

        //                    if (theaterId.HasValue)
        //                    {
        //                        query = query.Where(p => p.T_ID == theaterId.Value);
        //                    }

        //                    if (!string.IsNullOrEmpty(statusFilter))
        //                    {
        //                        query = query.Where(p => p.STATUS == statusFilter);
        //                    }

        //                    if (startDate.HasValue && endDate.HasValue)
        //                    {
        //                        query = query.Where(p => p.PAYMENT_DATE >= startDate.Value && p.PAYMENT_DATE <= endDate.Value);
        //                    }
        //                    else if (startDate.HasValue)
        //                    {
        //                        query = query.Where(p => p.PAYMENT_DATE >= startDate.Value);
        //                    }
        //                    else if (endDate.HasValue)
        //                    {
        //                        query = query.Where(p => p.PAYMENT_DATE <= endDate.Value);
        //                    }

        //                    if (minAmount.HasValue)
        //                    {
        //                        query = query.Where(p => p.AMOUNT >= minAmount.Value);
        //                    }

        //                    if (maxAmount.HasValue)
        //                    {
        //                        query = query.Where(p => p.AMOUNT <= maxAmount.Value);
        //                    }

        //                    // Convert to ViewModel and format date
        //                    var filteredPayments = query.ToList().Select(p => new PaymentHistoryViewModel
        //                    {
        //                        PAYMENT_ID = p.PAYMENT_ID,
        //                        T_ID = p.T_ID,
        //                        AMOUNT = p.AMOUNT,
        //                        STATUS = p.STATUS,
        //                        PAYMENT_DATE = p.PAYMENT_DATE.ToString("dd-MM-yyyy") // ✅ Correctly formatted date
        //                    }).ToList();

        //                    return View(filteredPayments);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                TempData["Error"] = "Database error: " + ex.Message;
        //                return RedirectToAction("Index");
        //            }
        //            finally
        //            {
        //                conn.Close(); // ✅ Ensure DB2 connection is closed
        //            }
        //        }
        //    }



        //[HttpGet]
        //public ActionResult AllReceipt()
        //{
        //    // Get current month and year
        //    int currentYear = DateTime.Now.Year;
        //    int currentMonth = DateTime.Now.Month;

        //    using (DB2Connection conn = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;"))
        //    {
        //        try
        //        {
        //            conn.Open(); // ✅ Open DB2 Connection

        //            using (var db = new ApplicationDbContext()) // Use Entity Framework context
        //            {
        //                // Fetch all approved theaters from DB
        //                var theaters = db.TRN_REGISTRATION
        //                    .Where(tr => tr.TStatus == "Approved" && tr.LicenseDate.HasValue)
        //                    .Select(tr => new
        //                    {
        //                        tr.TId,
        //                        tr.TName,
        //                        tr.TCity,
        //                        tr.TWard,
        //                        tr.TZone,
        //                        tr.TAddress,
        //                        tr.T_TENAMENT_NO,
        //                        tr.TStatus,
        //                        tr.LicenseDate
        //                    })
        //                    .ToList(); // Execute in memory

        //                // Create list to store each month's payment status
        //                var theaterDueList = new List<TheaterViewModel>();

        //                // Loop through each theater and generate month-wise payment status
        //                foreach (var theater in theaters)
        //                {
        //                    DateTime startDate = theater.LicenseDate.Value;
        //                    DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

        //                    if (startDate > currentDate)
        //                        continue; // Skip future start dates

        //                    // Generate months from LICENSE_DATE to current month
        //                    for (DateTime date = startDate; date <= currentDate; date = date.AddMonths(1))
        //                    {
        //                        string monthName = date.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
        //                        int year = date.Year;

        //                        bool isPaid = db.THEATER_TAX_PAYMENT
        //                            .Any(tp => tp.TId == theater.TId
        //                                    && tp.PaymentMonth == monthName
        //                                    && tp.PaymentYear == year);

        //                        if (isPaid)
        //                        {
        //                            // Fetch related receipts for this theater
        //                            var receipts = db.RECEIPT_FILTER
        //                                .Where(r => r.T_ID == theater.TId)
        //                                .Select(r => new ReceiptFilterViewModel
        //                                {
        //                                    RCPT_NO = r.RCPT_NO,
        //                                    RCPT_GEN_DATE = r.RCPT_GEN_DATE,
        //                                    T_ID = (int)r.T_ID,
        //                                    T_NAME = theater.TName,
        //                                    PAY_MODE = r.PAY_MODE,
        //                                    STATUS = r.STATUS
        //                                }).ToList();

        //                            theaterDueList.Add(new TheaterViewModel
        //                            {
        //                                T_ID = theater.TId,
        //                                T_NAME = theater.TName,
        //                                T_CITY = theater.TCity,
        //                                T_WARD = theater.TWard,
        //                                T_ZONE = theater.TZone,
        //                                T_ADDRESS = theater.TAddress,
        //                                T_TENAMENT_NO = theater.T_TENAMENT_NO.ToString(),
        //                                STATUS = theater.TStatus,
        //                                SINCE_MONTH = date.ToString("MMMM yyyy"), // Display as "March 2025"
        //                                PAYMENT_STATUS = "Paid",
        //                                Receipts = receipts // Assign the fetched receipts
        //                            });
        //                        }
        //                    }
        //                }

        //                return View(theaterDueList);
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


        //public ActionResult ActionRequests()
        //{
        //    // Query to fetch all data from TRN_REGISTRATION
        //    var query = from tr in db.TRN_REGISTRATION
        //                where tr.T_ACTIVE == true
        //                select new
        //                {
        //                    tr.T_ID,
        //                    tr.T_NAME,

        //                    tr.T_OWNER_NAME
        //                };


        //    // Execute the query and convert to a list
        //    var result = query.ToList();
        //    Console.WriteLine("Total Theaters Found: " + result.Count);

        //    // Convert the result to a list of ViewModel objects
        //    var theaterList = result.Select(tr => new ActionRequestViewModel
        //    {
        //        T_ID = tr.T_ID,
        //        T_NAME = tr.T_NAME,

        //        T_OWNER_NAME = tr.T_OWNER_NAME
        //    }).ToList();

        //    return View(theaterList);
        //}
        //[HttpPost]
        //public ActionResult ActionRequest()
        //{
        //    // Query to fetch all data from TRN_REGISTRATION
        //    var query = from tr in db.TRN_REGISTRATION
        //                where tr.T_ACTIVE == true
        //                select new
        //                {
        //                    tr.T_ID,
        //                    tr.T_NAME,

        //                    tr.T_OWNER_NAME
        //                };


        //    // Execute the query and convert to a list
        //    var result = query.ToList();
        //    Console.WriteLine("Total Theaters Found: " + result.Count);

        //    // Convert the result to a list of ViewModel objects
        //    var theaterList = result.Select(tr => new TheaterViewModel
        //    {
        //        T_ID = tr.T_ID,
        //        T_NAME = tr.T_NAME,

        //        T_OWNER_NAME = tr.T_OWNER_NAME
        //    }).ToList();

        //    return View(theaterList);
        //}

        [HttpPost]
        public ActionResult UpdateStatus(int theaterId, string status)
        {
            {
                var theater = db.TRN_REGISTRATION.FirstOrDefault(t => t.TId != null && t.TId.Trim() == theaterId.ToString());

                if (theater != null)
                {
                    // Update the status based on the provided value
                    theater.TStatus = status == "Approved" ? "Approved"
                                : status == "Rejected" ? "Rejected"
                                : "Pending"; // Default to "Pending"

                    db.SaveChanges(); // Save changes to the database
                }
            }

            // Redirect to the ActionRequests page after update
            return RedirectToAction("ActionRequests", "Department");
        }


        //public ActionResult GenerateReceipt(int id)
        //{
        //    // Try fetching the receipt directly
        //    var debugReceipt = _context.Receipts.FirstOrDefault(r => r.Id == id);

        //    if (debugReceipt == null)
        //    {
        //        throw new Exception($"Receipt with ID {id} not found in the database.");
        //    }

        //    System.Diagnostics.Debug.WriteLine($"Receipt Found: {debugReceipt.ReceiptNo}, Amount: {debugReceipt.Amount}");

        //    var receipt = _context.Receipts
        //        .Where(r => r.Id == id)
        //        .Select(r => new
        //        {
        //            r.ReceiptNo,
        //            r.Amount,
        //            r.PaymentMode,
        //            r.Description,
        //            T_ID = r.T_ID ?? 21,
        //            Theater = _context.TRN_REGISTRATION
        //                .Where(t => t.T_ID == (r.T_ID ?? 21))
        //                .Select(t => new
        //                {
        //                    t.T_NAME,
        //                    t.T_OWNER_EMAIL,
        //                    t.T_ADDRESS,
        //                    t.T_OWNER_NUMBER
        //                })
        //                .FirstOrDefault()
        //        })
        //        .FirstOrDefault();

        //    if (receipt == null)
        //    {
        //        TempData["ErrorMessage"] = "Receipt not found.";
        //        return RedirectToAction("Index");
        //    }

        //    var viewModel = new ReceiptViewModel
        //    {
        //        ReceiptNo = receipt.ReceiptNo ?? "N/A",
        //        Amount = receipt.Amount ?? 0,
        //        PaymentMode = receipt.PaymentMode ?? "N/A",
        //        TheaterName = receipt.Theater?.T_NAME ?? "N/A",
        //        Email = receipt.Theater?.T_OWNER_EMAIL ?? "N/A",
        //        Address = receipt.Theater?.T_ADDRESS ?? "N/A",
        //        T_OWNER_NUMBER = receipt.Theater?.T_OWNER_NUMBER?.ToString() ?? "N/A",
        //        Description = receipt.Description ?? "N/A",
        //        T_ID = receipt.T_ID
        //    };

        //    return View(viewModel);
        //}


        //public ActionResult Approve(int id)
        //{
        //    try
        //    {
        //        var theater = db.TRN_REGISTRATION.FirstOrDefault(t => t.T_ID == id);
        //        if (theater != null)
        //        {
        //            theater.STATUS = "Approved";  // Update status to "Approved"
        //            db.SaveChanges();  // Save the changes to the database
        //        }

        //        TempData["SuccessMessage"] = "Request Approved successfully!";
        //        return RedirectToAction("ActionRequests");  // Redirect back to the ActionRequests page
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error updating status: " + ex.Message;
        //        return RedirectToAction("ActionRequests");  // Redirect back to the ActionRequests page on error
        //    }
        //}

        //public ActionResult Reject(int id)
        //{
        //    try
        //    {
        //        var theater = db.TRN_REGISTRATION.FirstOrDefault(t => t.T_ID == id);
        //        if (theater != null)
        //        {
        //            theater.STATUS = "Rejected";  // Update status to "Rejected"
        //            db.SaveChanges();  // Save the changes to the database
        //        }

        //        TempData["SuccessMessage"] = "Request Rejected successfully!";
        //        return RedirectToAction("ActionRequests");  // Redirect back to the ActionRequests page
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error updating status: " + ex.Message;
        //        return RedirectToAction("ActionRequests");  // Redirect back to the ActionRequests page on error
        //    }
        //}

        //public ActionResult Tax_Hold()
        //{
        //    return View();
        //}


        //    public JsonResult GetTheaters(string term)
        //{
        //    var theaters = db.TRN_REGISTRATION
        //                      .Where(t => t.T_NAME.Contains(term)) // Search by theater name
        //                      .Select(t => new
        //                      {
        //                          Id = t.T_ID,  // Theater ID
        //                          T_NAME = t.T_NAME, // Theater Name
        //                          //T_OWNER_NAME = t.T_OWNER_NAME // Owner Name
        //                      })
        //                      .ToList();

        //    return Json(theaters, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult AllReceipt()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

            try
            {
                using (var db = new ApplicationDbContext()) // Use Entity Framework context
                {
                    // ✅ Fetch all approved theaters
                    var theaters = db.TRN_REGISTRATION
                        .Where(tr => tr.TStatus == "Approved")
                        .Select(tr => new
                        {
                            tr.ApplId,
                            tr.TName,
                            tr.TCity,
                            tr.TWard,
                            tr.TZone,
                            tr.TAddress,
                            tr.TTenamentNo,
                            tr.TStatus,
                            LicenseDate = tr.LicenseDate ?? new DateTime(2000, 1, 1) // ✅ Default if null
                        })
                        .ToList();

                    // ✅ Fetch all payments at once (avoid multiple queries in loop)
                    var allPayments = db.THEATER_TAX_PAYMENT
                        .Select(tp => new { tp.ApplId, tp.PaymentMonth, tp.PaymentYear })
                        .ToList();

                    var theaterDueList = new List<TheaterViewModel>();

                    foreach (var theater in theaters)
                    {
                        DateTime startDate = theater.LicenseDate > currentDate ? currentDate : theater.LicenseDate;

                        for (DateTime date = startDate; date <= currentDate; date = date.AddMonths(1))
                        {
                            string monthName = date.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
                            int year = date.Year;

                            bool isPaid = allPayments.Any(tp => tp.ApplId == theater.ApplId
                                && tp.PaymentMonth == monthName
                                && tp.PaymentYear == year);

                            if (!isPaid) continue; // ✅ Skip unpaid theaters

                            theaterDueList.Add(new TheaterViewModel
                            {
                                ApplId = theater.ApplId,
                                T_NAME = theater.TName,
                                T_CITY = theater.TCity,
                                T_WARD = theater.TWard,
                                T_ZONE = theater.TZone,
                                T_ADDRESS = theater.TAddress,
                                T_TENAMENT_NO = theater.TTenamentNo.ToString(),
                                T_STATUS = theater.TStatus,
                                SINCE_MONTH = date.ToString("MMMM yyyy"),
                                PAYMENT_STATUS = "Paid",
                                RCPT_NO = "Generated",
                                RCPT_GEN_DATE = DateTime.Now,
                                PAY_MODE = "Cash",
                                STATUS = "Paid"
                            });
                        }
                    }

                    return View(theaterDueList); // ✅ Only Paid theaters are passed to the view
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Database error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public ActionResult AllReceipt(int? theaterId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                using (var db = new ApplicationDbContext()) // Use Entity Framework context
                {
                    var theatersQuery = db.TRN_REGISTRATION
                        .Where(tr => tr.TStatus == "Approved");

                    if (theaterId.HasValue)
                    {
                        theatersQuery = theatersQuery.Where(tr => tr.ApplId == theaterId);
                    }

                    var theaters = theatersQuery
                        .Select(tr => new
                        {
                            tr.ApplId,
                            tr.TName,
                            tr.TCity,
                            tr.TWard,
                            tr.TZone,
                            tr.TAddress,
                            tr.TTenamentNo,
                            tr.TStatus,
                            LicenseDate = tr.LicenseDate ?? new DateTime(2000, 1, 1) // ✅ Default if null
                        })
                        .ToList();

                    var allPayments = db.THEATER_TAX_PAYMENT
                        .Select(tp => new { tp.ApplId, tp.PaymentMonth, tp.PaymentYear })
                        .ToList();

                    var theaterDueList = new List<TheaterViewModel>();

                    foreach (var theater in theaters)
                    {
                        DateTime startDate = theater.LicenseDate;
                        DateTime endDate = DateTime.Now;

                        if (fromDate.HasValue && toDate.HasValue)
                        {
                            startDate = fromDate.Value;
                            endDate = toDate.Value;
                        }

                        for (DateTime date = startDate; date <= endDate; date = date.AddMonths(1))
                        {
                            string monthName = date.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
                            int year = date.Year;

                            bool isPaid = allPayments.Any(tp => tp.ApplId == theater.ApplId
                                && tp.PaymentMonth == monthName
                                && tp.PaymentYear == year);

                            theaterDueList.Add(new TheaterViewModel
                            {
                                ApplId = theater.ApplId,
                                T_NAME = theater.TName,
                                T_CITY = theater.TCity,
                                T_WARD = theater.TWard,
                                T_ZONE = theater.TZone,
                                T_ADDRESS = theater.TAddress,
                                T_TENAMENT_NO = theater.TTenamentNo.ToString(),
                                T_STATUS = theater.TStatus,
                                SINCE_MONTH = date.ToString("MMMM yyyy"),
                                PAYMENT_STATUS = isPaid ? "Paid" : "Unpaid",
                                RCPT_NO = isPaid ? "Generated" : "N/A",
                                RCPT_GEN_DATE = isPaid ? DateTime.Now : (DateTime?)null,
                                PAY_MODE = isPaid ? "Cash" : "N/A",
                                STATUS = isPaid ? "Paid" : "Pending"
                            });
                        }
                    }

                    return View("AllReceipt", theaterDueList);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Database error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


    }
}

