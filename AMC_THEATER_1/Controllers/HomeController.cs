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
using System.Runtime.Remoting.Messaging;
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
        readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult ReceiptFormat()
        {
            return View();
        }
        public ActionResult Complain_Grivance()
        {
            return View();
        }

        public ActionResult Confirm_Payment(string id)
        {
            //if (id == null)
            //{
            //    TempData["Error"] = "Invalid Theater ID.";
            //    return RedirectToAction("Index");
            //}

            var registration = db.TRN_REGISTRATION.FirstOrDefault(r => r.TId == id);

            if (registration != null)
            {
               var latestAmt = db.THEATER_TAX_PAYMENT
                 .Where(t => t.TId == registration.TId)
                 .OrderByDescending(t => t.CreateDate) // Ensure the latest record is selected
                 .Select(t => t.TaxAmount)
                 .FirstOrDefault(); // Fetch the latest amount

ViewBag.TotalAmount = latestAmt;


                var viewModel = new TheaterViewModel
                {
                    T_ID = registration.TId,
                    T_NAME = registration.TName
                };

                return View(viewModel);
            }

            ViewBag.TotalAmount = 0;
            return View(new TheaterViewModel()); // Return an empty ViewModel if no data is found
        }

        public ActionResult Print_Application(int? id, string mode = "print")
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid Application ID!";
                return RedirectToAction("List_of_Application", "Home");
            }

            var registration = db.TRN_REGISTRATION.FirstOrDefault(r => r.ApplId == id);
            if (registration == null)
            {
                TempData["ErrorMessage"] = "No record found!";
                return RedirectToAction("List_of_Application", "Home");
            }

            // ✅ Fetch uploaded documents with their names
            var uploadedDocs = (from d in db.TRN_THEATRE_DOCS
                                join m in db.MST_DOCS on d.DocId equals m.DocId
                                where d.ApplId == id && !string.IsNullOrEmpty(d.DocFilePath)
                                select new UploadedDocViewModel
                                {
                                    DocId = d.DocId,
                                    DocFilePath = d.DocFilePath,
                                    DocName = m.DocName
                                }).ToList();

            ViewBag.UploadedDocs = uploadedDocs;

            // ✅ Map TRN_REGISTRATION to TheaterViewModel
            var viewModel = new TheaterViewModel
            {
                ApplId = registration.ApplId,
                T_ID = registration.TId,
                T_NAME = registration.TName,
                T_CITY = registration.TCity,
                T_TENAMENT_NO = registration.TTenamentNo,
                T_WARD = registration.TWard,
                T_ZONE = registration.TZone,
                T_OWNER_NAME = registration.LicenseHolderName,
                T_OWNER_EMAIL = registration.TEmail,
                T_COMMENCEMENT_DATE = registration.TCommencementDate,
                T_ADDRESS = registration.TAddress,
                CreateDate = registration.CreateDate,
                ManagerName = registration.ManagerName,
                T_PEC_NO = registration.TPecNo,
                T_PRC_NO= registration.TPrcNo,
                LicenseDate = registration.LicenseDate,
                T_OWNER_NUMBER = registration.ManagerContactNo,

                // Fetch related data
                Screens = db.NO_OF_SCREENS
                            .Where(s => s.ApplId == id)
                            .Select(s => new ScreenViewModel
                            {
                                ScreenNo = s.ScreenNo,
                                ScreenType = s.ScreenType,
                                AudienceCapacity = s.AudienceCapacity
                            })
                            .ToList(),

                ScreenTypes = db.MST_TT_TYPE.ToList(),
                IsEditMode = (mode == "print"),
            };

            ViewBag.Mode = mode;
            return View("Print_Application", viewModel);
        }

        public ActionResult List_of_Application()
        {
            ViewBag.CurrentAction = "List of Application"; // ✅ Important for UI
            List<TheaterViewModel> theaterList = new List<TheaterViewModel>();

            try
            {
                // Retrieve the phone number from the session
                string sessionPhoneNumber = Session["PhoneNumber"]?.ToString();

                if (string.IsNullOrEmpty(sessionPhoneNumber))
                {
                    TempData["ErrorMessage"] = "Session expired or phone number missing. Please log in again.";
                    return RedirectToAction("Login", "Home");
                }

                // Fetch data from TRN_REGISTRATION and calculate screen counts from NO_OF_SCREENS
                var query = from tr in db.TRN_REGISTRATION
                            where tr.TActive == 1 && tr.ManagerContactNo.ToString() == sessionPhoneNumber
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

                                //// Count Theater Screens
                                //TheaterScreenCount = (from s in db.NO_OF_SCREENS
                                //                      where s.ApplId == tr.ApplId && s.ScreenType == "Theater"
                                //                      select s.ScreenId).Count(),

                                //// Count Video Screens
                                //VideoScreenCount = (from s in db.NO_OF_SCREENS
                                //                    where s.ApplId == tr.ApplId && s.ScreenType == "Video"
                                //                    select s.ScreenId).Count()
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

                    //// Set the screen count values
                    //SCREEN_COUNT = tr.TheaterScreenCount + tr.VideoScreenCount,
                    //THEATER_SCREEN_COUNT = tr.TheaterScreenCount,
                    //VIDEO_THEATER_SCREEN_COUNT = tr.VideoScreenCount
                }).ToList();

          
            }
            catch (Exception ex)
            {
                // 🔴 Log the detailed error, including inner exceptions
                var errorMessage = $"List_of_Application Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner Exception: {ex.InnerException.Message}";
                }
                // Log error message (replace with your logger)
                System.Diagnostics.Debug.WriteLine(errorMessage);

                TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
                return RedirectToAction("List_of_Application", "Home");
            }

            return View(theaterList);
        }


        public ActionResult ActionRequests()
        {
            ViewBag.CurrentAction = "ActionRequests"; // ✅ Important for UI

            List<TheaterViewModel> theaterList = new List<TheaterViewModel>();

            try
            {
                var query = from tr in db.TRN_REGISTRATION
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
                                tr.TCommencementDate
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
                    UPDATE_DATE = tr.UpdateDate ?? DateTime.MinValue
                }).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message; // ✅ Capture DB2 errors
            }

            return View(theaterList);
        }



        public ActionResult Theater_Tax()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Theater_Tax(string theater_id)
        {
            // ✅ Initialize ViewModel with default values
            var model = new TaxPaymentViewModel
            {
                TId = theater_id,
                FromMonth = DateTime.Now.ToString("MMMM"), // ✅ Default to current month
                ToMonth = DateTime.Now.Year.ToString() // ✅ Convert int to string
            };

            // ✅ Step 1: Fetch screen prices first and store in memory
            var screenPrices = db.MST_TT_TYPE
                .AsNoTracking() // ✅ Improves performance
                .ToDictionary(p => p.ScreenType, p => p.ScreenPrice);

            // ✅ Step 2: Fetch only Approved theater details
            var theater = db.TRN_REGISTRATION
                .Where(t => t.TId == theater_id && t.TStatus == "Approved") // ✅ Filter for Approved status
                .FirstOrDefault();

            if (theater == null)
            {
                TempData["Error"] = "Theater ID not found or not approved.";
                return RedirectToAction("Index");
            }

            // ✅ Step 3: Fetch Screens Separately
            var screens = db.NO_OF_SCREENS
                .Where(s => s.TId == theater.TId)
                .ToList(); // ✅ Fetch screens separately

            // ✅ Step 4: Map data to ViewModel
            model.Screens = screens.Select(s => new ScreenViewModel
            {
                ScreenId = s.ScreenId,
                AudienceCapacity = (int)s.AudienceCapacity,
                ScreenType = s.ScreenType,
                ScreenNo = (int)s.ScreenNo,
                ScreenPrice = screenPrices.ContainsKey(s.ScreenType)
                              ? Convert.ToInt32(screenPrices[s.ScreenType])
                              : 0 // ✅ Fallback value
            }).ToList();

            model.TheaterName = theater.TName;
            model.MobileNo = theater.ManagerContactNo != null ? theater.ManagerContactNo.ToString() : string.Empty;
            model.Address = theater.TAddress;
            model.Email = theater.TEmail;
            model.ApplId = theater.ApplId;

            return View(model);
        }


        [HttpPost]
        public ActionResult ProcessTaxPayment(TaxPaymentViewModel model, HttpPostedFileBase DocumentPath, FormCollection form)
        {
           
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
                                TId = model.TId.ToString(),
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
                                    TId = model.TId.ToString(),
                                    ApplId = model.ApplId,

                                    TaxId = taxPayment.TaxId,
                                    ScreenType = screen.ScreenType ?? "Unknown",
                                    TotalShow = screen.TotalShow,
                                    AudienceCapacity = screen.AudienceCapacity.HasValue ? (int)screen.AudienceCapacity : 0,
                                    CancelShow = screen.CancelShow,
                                    ActualShow = Math.Max(0, screen.TotalShow - screen.CancelShow),
                                    RatePerScreen = (screen.ScreenType == "Theater") ? 75 : 25,
                                    AmountPerScreen = screen.AmtPerScreen
                                };

                                db.NO_OF_SCREENS_TAX.Add(screenTax);
                            }
                        }

                        db.SaveChanges();
                TempData["Success"] = "Tax payment saved successfully!";
                return RedirectToAction("Confirm_Payment", "Home", new { id = model.TId });
               
                
            }
        }
        [HttpGet]
        public JsonResult GetTheaterSuggestions(string query)
        {
            var theaters = db.TRN_REGISTRATION
                             .Where(t => (t.TId.StartsWith(query) || t.TName.Contains(query))
                                         && t.TStatus == "Approved") // Filter only Approved TStatus
                             .Select(t => new { t.TId, t.TName })
                             .ToList();

            return Json(theaters, JsonRequestBehavior.AllowGet);
        }




        public ActionResult Seprate_Theater_Tax()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Seprate_Theater_Tax(string theater_id)
        {
            // ✅ Initialize ViewModel with default values
            var model = new TaxPaymentViewModel
            {
                TId = theater_id,
                FromMonth = DateTime.Now.ToString("MMMM"), // ✅ Default to current month
                ToMonth = DateTime.Now.Year.ToString() // ✅ Convert int to string
            };

            // ✅ Step 1: Fetch screen prices first and store in memory
            var screenPrices = db.MST_TT_TYPE
                .AsNoTracking() // ✅ Improves performance
                .ToDictionary(p => p.ScreenType, p => p.ScreenPrice);

            // ✅ Step 2: Fetch only Approved theater details
            var theater = db.TRN_REGISTRATION
                .Where(t => t.TId == theater_id && t.TStatus == "Approved") // ✅ Filter for Approved status
                .FirstOrDefault();

            if (theater == null)
            {
                TempData["Error"] = "Theater ID not found or not approved.";
                return RedirectToAction("Index");
            }

            // ✅ Step 3: Fetch Screens Separately
            var screens = db.NO_OF_SCREENS
                .Where(s => s.TId == theater.TId)
                .ToList(); // ✅ Fetch screens separately

            // ✅ Step 4: Map data to ViewModel
            model.Screens = screens.Select(s => new ScreenViewModel
            {
                ScreenId = s.ScreenId,
                AudienceCapacity = (int)s.AudienceCapacity,
                ScreenType = s.ScreenType,
                ScreenNo = (int)s.ScreenNo,
                ScreenPrice = screenPrices.ContainsKey(s.ScreenType)
                              ? Convert.ToInt32(screenPrices[s.ScreenType])
                              : 0 // ✅ Fallback value
            }).ToList();

            model.TheaterName = theater.TName;
            model.MobileNo = theater.ManagerContactNo != null ? theater.ManagerContactNo.ToString() : string.Empty;
            model.Address = theater.TAddress;
            model.Email = theater.TEmail;
            model.ApplId = theater.ApplId;

            return View(model);
        }

        public ActionResult ProcessTaxPaymentSeprate(TaxPaymentViewModel model, HttpPostedFileBase DocumentPath)
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
                    TId = model.TId.ToString(),
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
                        TId = model.TId.ToString(),
                        ApplId = model.ApplId,

                        TaxId = taxPayment.TaxId,
                        ScreenType = screen.ScreenType ?? "Unknown",
                        TotalShow = screen.TotalShow,
                        AudienceCapacity = screen.AudienceCapacity.HasValue ? (int)screen.AudienceCapacity : 0,
                        CancelShow = screen.CancelShow,
                        ActualShow = Math.Max(0, screen.TotalShow - screen.CancelShow),
                        RatePerScreen = (screen.ScreenType == "Theater") ? 75 : 25,
                        AmountPerScreen = screen.AmtPerScreen
                    };

                    db.NO_OF_SCREENS_TAX.Add(screenTax);
                }
            }

            db.SaveChanges();
            return RedirectToAction("Confirm_Payment", "Home", new { id = model.TId });

        }
        public ActionResult DeptHomePage()
        {


            return View();
        }


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
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            var theaters = db.TRN_REGISTRATION
                .Where(tr => tr.TActive.Value == 1 && tr.TStatus == "Approved")
                .Select(tr => new
                {
                    tr.TId,
                    tr.TName,
                    tr.TCity,
                    tr.TAddress,
                    tr.TTenamentNo,
                    tr.TZone,
                    tr.TWard,
                    tr.TStatus,
                    tr.RejectReason,
                    tr.UpdateDate,
                    tr.TCommencementDate
                })
                .ToList();

            var theaterList = new List<TheaterViewModel>();

            foreach (var theater in theaters)
            {
                if (!theater.TCommencementDate.HasValue) continue; // Skip if no commencement date

                DateTime startDate = theater.TCommencementDate.Value;
                DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

                for (DateTime date = startDate; date <= currentDate; date = date.AddMonths(1))
                {
                    string monthName = date.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
                    int year = date.Year;

                    bool isPaid = db.THEATER_TAX_PAYMENT.Any(tp => tp.TId == theater.TId && tp.PaymentMonth == monthName && tp.PaymentYear == year);

                    if (!isPaid) // ✅ Show only NOT PAID records
                    {
                        theaterList.Add(new TheaterViewModel
                        {
                            T_ID = theater.TId,
                            T_NAME = theater.TName,
                            T_CITY = theater.TCity,
                            T_ADDRESS = theater.TAddress,
                            T_TENAMENT_NO = theater.TTenamentNo,
                            T_ZONE = theater.TZone,
                            T_WARD = theater.TWard,
                            T_STATUS = theater.TStatus,
                            REJECT_REASON = theater.RejectReason,
                            T_COMMENCEMENT_DATE = startDate,
                            UPDATE_DATE = theater.UpdateDate ?? DateTime.MinValue,
                            SINCE_MONTH = date.ToString("MMMM yyyy"),
                            PAYMENT_STATUS = "Not Paid"
                        });
                    }
                }
            }

            return View(theaterList);
        }

        [HttpPost]
        public ActionResult PendingDuesDept(string theaterId, string fromDate, string toDate, string cityFilter, string wardFilter, string zoneFilter, string statusFilter)
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            var theaters = db.TRN_REGISTRATION
                .Where(tr => tr.TStatus == "Approved" && tr.CreateDate.HasValue && (tr.TActive.HasValue && tr.TActive.Value == 1))
                .Select(tr => new
                {
                    tr.TId,  // ✅ TId is already a string in the database
                    tr.TName,
                    tr.TCity,
                    tr.TWard,
                    tr.TZone,
                    tr.TAddress,
                    tr.TTenamentNo,
                    tr.TStatus,
                    tr.LicenseDate
                })
                .ToList();

            var theaterDueList = new List<TheaterViewModel>();

            foreach (var theater in theaters)
            {
                DateTime startDate = theater.LicenseDate.Value;
                DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

                if (startDate > currentDate) continue;

                for (DateTime date = startDate; date <= currentDate; date = date.AddMonths(1))
                {
                    string monthName = date.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
                    int year = date.Year;

                    bool isPaid = db.THEATER_TAX_PAYMENT.Any(tp => tp.TId == theater.TId && tp.PaymentMonth == monthName && tp.PaymentYear == year);

                    if (!isPaid)
                    {
                        theaterDueList.Add(new TheaterViewModel
                        {
                            T_ID = theater.TId,  // ✅ TId remains a string
                            T_NAME = theater.TName,
                            T_CITY = theater.TCity,
                            T_WARD = theater.TWard,
                            T_ZONE = theater.TZone,
                            T_ADDRESS = theater.TAddress,
                            T_TENAMENT_NO = theater.TTenamentNo,
                            T_STATUS = theater.TStatus,
                            SINCE_MONTH = date.ToString("MMMM yyyy"),
                            PAYMENT_STATUS = "Not Paid"
                        });
                    }
                }
            }

            // ✅ Apply Filters
            if (!string.IsNullOrEmpty(theaterId))
            {
                theaterDueList = theaterDueList.Where(t => t.T_ID.Equals(theaterId, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime fromDt))
            {
                theaterDueList = theaterDueList.Where(t => DateTime.Parse(t.SINCE_MONTH) >= fromDt).ToList();
            }

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime toDt))
            {
                theaterDueList = theaterDueList.Where(t => DateTime.Parse(t.SINCE_MONTH) <= toDt).ToList();
            }

            if (!string.IsNullOrEmpty(cityFilter))
            {
                theaterDueList = theaterDueList.Where(t => t.T_CITY.Equals(cityFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(wardFilter))
            {
                theaterDueList = theaterDueList.Where(t => t.T_WARD.Equals(wardFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(zoneFilter))
            {
                theaterDueList = theaterDueList.Where(t => t.T_ZONE.Equals(zoneFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                theaterDueList = theaterDueList.Where(t => t.T_STATUS.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(theaterDueList);
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
                    .Where(tr => tr.TStatus == "Approved" && tr.CreateDate.HasValue) // Ensure LICENSE_DATE exists
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

        [HttpPost]
        public ActionResult PaymentList(string theaterId, string fromDate, string toDate, string cityFilter, string wardFilter, string zoneFilter, string statusFilter)
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            var theaters = db.TRN_REGISTRATION
                .Where(tr => tr.TStatus == "Approved" && tr.CreateDate.HasValue && (tr.TActive.HasValue && tr.TActive.Value == 1))
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
                .ToList();

            var theaterPaidList = new List<TheaterViewModel>();

            foreach (var theater in theaters)
            {
                DateTime startDate = theater.LicenseDate.Value;
                DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

                if (startDate > currentDate) continue;

                for (DateTime date = startDate; date <= currentDate; date = date.AddMonths(1))
                {
                    string monthName = date.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
                    int year = date.Year;

                    bool isPaid = db.THEATER_TAX_PAYMENT.Any(tp => tp.TId == theater.TId && tp.PaymentMonth == monthName && tp.PaymentYear == year);

                    if (isPaid)
                    {
                        theaterPaidList.Add(new TheaterViewModel
                        {
                            T_ID = theater.TId,
                            T_NAME = theater.TName,
                            T_CITY = theater.TCity,
                            T_WARD = theater.TWard,
                            T_ZONE = theater.TZone,
                            T_ADDRESS = theater.TAddress,
                            T_TENAMENT_NO = theater.TTenamentNo,
                            T_STATUS = theater.TStatus,
                            SINCE_MONTH = date.ToString("MMMM yyyy"),
                            PAYMENT_STATUS = "Paid"
                        });
                    }
                }
            }

            // Apply Filters
            if (!string.IsNullOrEmpty(theaterId))
            {
                theaterPaidList = theaterPaidList.Where(t => t.T_ID.Equals(theaterId, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime fromDt))
            {
                theaterPaidList = theaterPaidList.Where(t => DateTime.Parse(t.SINCE_MONTH) >= fromDt).ToList();
            }

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime toDt))
            {
                theaterPaidList = theaterPaidList.Where(t => DateTime.Parse(t.SINCE_MONTH) <= toDt).ToList();
            }

            if (!string.IsNullOrEmpty(cityFilter))
            {
                theaterPaidList = theaterPaidList.Where(t => t.T_CITY.Equals(cityFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(wardFilter))
            {
                theaterPaidList = theaterPaidList.Where(t => t.T_WARD.Equals(wardFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(zoneFilter))
            {
                theaterPaidList = theaterPaidList.Where(t => t.T_ZONE.Equals(zoneFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                theaterPaidList = theaterPaidList.Where(t => t.T_STATUS.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(theaterPaidList);
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
            try
            {
                // Ensure the model's PhoneNumber is not null or empty
                if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Phone number is required.");
                    return View(model);
                }

                // Check if the phone number exists in the database
                bool userExists = db.USER_LOGIN_DETAILS.Any(u => u.PhoneNumber == model.PhoneNumber);

                if (!userExists)
                {
                    ModelState.AddModelError("PhoneNumber", "Phone number not found.");
                    return View(model);
                }

                // ✅ Store phone number in Session as a string
                Session["PhoneNumber"] = model.PhoneNumber;

                return RedirectToAction("Registration", "Registration");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Inner Exception: " + ex.InnerException?.Message);
                throw; // Rethrow the exception for debugging
            }
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
                                       string theaterTypeFilter, string deleteId)
        {
            Debug.WriteLine($"Received Data -> FromDate: {fromDate}, ToDate: {toDate}");

            if (!string.IsNullOrEmpty(deleteId))
            {
                var theaterToDelete = db.TRN_REGISTRATION.FirstOrDefault(t => t.TId == deleteId);
                if (theaterToDelete != null)
                {
                    theaterToDelete.TActive = 0;
                    db.SaveChanges();
                }
            }

            var statuses = new List<string> { "Pending", "Approved", "Reject" };
            var query = db.TRN_REGISTRATION.Where(tr => tr.TActive.HasValue && tr.TActive.Value == 1);

            if (!string.IsNullOrEmpty(theaterId))
            {
                query = query.Where(tr => tr.TId == theaterId);
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
                tr.ApplId,
                tr.TId, // Now using TId instead of ApplId
                tr.TName,
                tr.TCity,
                tr.TAddress,
                tr.TTenamentNo,
                tr.TZone,
                tr.TWard,
                tr.TCommencementDate,
                tr.TStatus,
                tr.UpdateDate,
            }).ToList();

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            ViewBag.Cities = result.Select(tr => tr.TCity).Distinct().ToList();
            ViewBag.Wards = result.Select(tr => tr.TWard).Distinct().ToList();
            ViewBag.Zones = result.Select(tr => tr.TZone).Distinct().ToList();
            ViewBag.Statuses = statuses;

            ViewBag.TheaterTypes = db.MST_TT_TYPE.Select(t => t.ScreenType).Distinct().ToList();

            var theaterList = result.Select(tr => new TheaterViewModel
            {
                ApplId=tr.ApplId,                
                T_ID = tr.TId, // Updated to use string TId
                T_NAME = tr.TName,
                T_CITY = tr.TCity,
                T_ADDRESS = tr.TAddress,
                T_TENAMENT_NO = tr.TTenamentNo?.ToString(),
                T_WARD = tr.TWard,
                T_ZONE = tr.TZone,
                T_COMMENCEMENT_DATE = tr.TCommencementDate ?? DateTime.MinValue, // Handling null values safely
                T_STATUS = tr.TStatus,
                UPDATE_DATE = tr.UpdateDate ?? DateTime.MinValue,
            }).ToList();

            return View(theaterList);
        }

        public ActionResult Dues()
        {
            // Get current month and year
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

       
                    {
                        // Fetch all approved theaters from DB
                        var theaters = db.TRN_REGISTRATION
                            .Where(tr => tr.TStatus == "Approved" && tr.CreateDate.HasValue && (tr.TActive.HasValue && tr.TActive.Value == 1)) // ✅ Compare with 1 instead of true
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


        public ActionResult PaymentHistoryFilter()
        {

            return View();
        }

        public ActionResult Home_Page()
        {
            return View();
        }

     
        [HttpGet]
        public ActionResult AllReceipt()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            DateTime currentDate = new DateTime(currentYear, currentMonth, 1);

            
                {
                    // ✅ Fetch all approved theaters
                    var theaters = db.TRN_REGISTRATION
                        .Where(tr => tr.TStatus == "Approved")
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
                            CreateDate = tr.CreateDate ?? new DateTime(2000, 1, 1) // ✅ Default if null
                        })
                        .ToList();

                    // ✅ Fetch all payments at once (avoid multiple queries in loop)
                    var allPayments = db.THEATER_TAX_PAYMENT
                        .Select(tp => new { tp.ApplId, tp.PaymentMonth, tp.PaymentYear })
                        .ToList();

                    var theaterDueList = new List<TheaterViewModel>();

                    foreach (var theater in theaters)
                    {
                        DateTime startDate = theater.CreateDate > currentDate ? currentDate : theater.CreateDate;

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
                                T_ID = theater.TId,
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
          


        [HttpPost]
        public ActionResult AllReceipt(int? theaterId, DateTime? fromDate, DateTime? toDate)
        {
           
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
                            CreateDate = tr.CreateDate ?? new DateTime(2000, 1, 1) // ✅ Default if null
                        })
                        .ToList();

                    var allPayments = db.THEATER_TAX_PAYMENT
                        .Select(tp => new { tp.ApplId, tp.PaymentMonth, tp.PaymentYear })
                        .ToList();

                    var theaterDueList = new List<TheaterViewModel>();

                    foreach (var theater in theaters)
                    {
                        DateTime startDate = theater.CreateDate;
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
        public ActionResult Notice()
        {
            return View();
        }
        public ActionResult Pending_Payment()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            DateTime currentDate = new DateTime(currentYear, currentMonth, 1);


            {
                // ✅ Fetch all approved theaters
                var theaters = db.TRN_REGISTRATION
                    .Where(tr => tr.TStatus == "Approved")
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
                        CreateDate = tr.CreateDate ?? new DateTime(2000, 1, 1) // ✅ Default if null
                    })
                    .ToList();

                // ✅ Fetch all payments at once (avoid multiple queries in loop)
                var allPayments = db.THEATER_TAX_PAYMENT
                    .Select(tp => new { tp.ApplId, tp.PaymentMonth, tp.PaymentYear })
                    .ToList();

                var theaterDueList = new List<TheaterViewModel>();

                foreach (var theater in theaters)
                {
                    DateTime startDate = theater.CreateDate > currentDate ? currentDate : theater.CreateDate;

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
                            T_ID = theater.TId,
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

        [HttpPost]
        public ActionResult Pending_Payment(int? theaterId, DateTime? fromDate, DateTime? toDate)
        {
           
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
                            CreateDate = tr.CreateDate ?? new DateTime(2000, 1, 1) // ✅ Default if null
                        })
                        .ToList();

        var allPayments = db.THEATER_TAX_PAYMENT
            .Select(tp => new { tp.ApplId, tp.PaymentMonth, tp.PaymentYear })
            .ToList();

        var theaterDueList = new List<TheaterViewModel>();

                    foreach (var theater in theaters)
                    {
                        DateTime startDate = theater.CreateDate;
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
        }
    
    }



