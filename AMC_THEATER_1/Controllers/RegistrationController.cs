using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AMC_THEATER_1.Models;
using System.Data.Entity;
using IBM.Data.DB2;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Data;
using System.IO; // ✅ Import DB2 namespace

namespace AMC_THEATER_1.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly string db2ConnectionString = "Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;";
        private readonly DB2Connection db2Connection = new DB2Connection("Database=prddb1;uid=prdinst1;pwd=prdinst1;Server=123.63.211.14:50000;");
       
        public ActionResult Edit(int id, string mode = "edit")
        {
            try
            {
                var model = db.TRN_REGISTRATION
                              .Where(r => r.ApplId == id) // Ensure correct filtering
                              .Include(r => r.NO_OF_SCREENS)
                              .Include(r => r.TRN_THEATRE_DOCS)
                              .FirstOrDefault();

                if (model == null)
                {
                    TempData["ErrorMessage"] = "No record found!";
                    return RedirectToAction("List_of_Application", "Home");
                }

                // ✅ Ensure lists are initialized
                model.NO_OF_SCREENS = model.NO_OF_SCREENS ?? new List<NO_OF_SCREENS>();
                model.TRN_THEATRE_DOCS = model.TRN_THEATRE_DOCS ?? new List<TRN_THEATRE_DOCS>();

                // ✅ Fetch active documents
                ViewBag.Documents = GetActiveDocuments() ?? new List<MST_DOCS>();

                // ✅ Fetch uploaded documents
                ViewBag.UploadedDocs = GetUploadedDocs(id);

                // ✅ Ensure ViewBag.Mode is properly set
                ViewBag.Mode = mode;
                model.IsEditMode = (mode == "edit");

                return View("Registration", model);
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
        }

        private List<TRN_THEATRE_DOCS> GetUploadedDocs(int applId)
        {
            try
            {
                return db.TRN_THEATRE_DOCS
                         .Where(d => d.ApplId == applId)
                         .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUploadedDocs Error: {ex.Message}");
                return new List<TRN_THEATRE_DOCS>();
            }
        }




        // ✅ GET: Registration (Load Registration Form)
        public ActionResult Registration(int? id, bool isViewPage = false, string mode = "create")
        {
            ViewBag.PageTitle = "Theater Registration";
            ViewBag.Mode = mode;
            ViewBag.Documents = GetActiveDocuments();

            var model = new TRN_REGISTRATION();

            if (id.HasValue)
            {
                using (DB2Connection db2Conn = new DB2Connection(db2ConnectionString))
                {
                    try
                    {
                        db2Conn.Open(); // ✅ Open DB2 Connection

                        var registrationData = db.TRN_REGISTRATION
                            .Include(r => r.NO_OF_SCREENS)
                            .Include(r => r.TRN_THEATRE_DOCS)
                            .FirstOrDefault(r => r.ApplId == id.Value);

                        if (registrationData == null)
                        {
                            TempData["ErrorMessage"] = "No registration data found!";
                            return RedirectToAction("Index");
                        }

                        model = registrationData;
                        ViewBag.Screens = model.NO_OF_SCREENS?.ToList() ?? new List<NO_OF_SCREENS>();
                        ViewBag.UploadedDocs = model.TRN_THEATRE_DOCS?.ToList() ?? new List<TRN_THEATRE_DOCS>();
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "DB2 Connection Error: " + ex.Message;
                    }
                    finally
                    {
                        db2Conn.Close(); // ✅ Ensure DB2 connection is closed
                    }
                }
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(TRN_REGISTRATION model, HttpPostedFileBase[] documents, string actionType, string rejectReason = null)
        {
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    db2Connection.Open();

                    if (model.ApplId == 0) // New Registration
                    {
                        if (!string.IsNullOrEmpty(Request.Form["OfflineTaxPaidYear"]))
                        {
                            if (int.TryParse(Request.Form["OfflineTaxPaidYear"], out int year))
                            {
                                model.OfflineTaxPaidYear = new DateTime(year, 1, 1);
                                System.Diagnostics.Debug.WriteLine("Parsed OfflineTaxPaidYear: " + model.OfflineTaxPaidYear);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Year parsing failed.");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("OfflineTaxPaidYear is null or empty.");
                        }


                        model.TActive = 1; // Ensure column name matches DB2 schema
                        model.TStatus = "Pending";
                        db.TRN_REGISTRATION.Add(model);
                        db.SaveChanges();
                    }
                    else // Update existing record
                    {
                        var existingEntity = db.TRN_REGISTRATION.Find(model.ApplId);
                        if (existingEntity == null)
                        {
                            TempData["ErrorMessage"] = "No record found for the provided ID.";
                            return View(model);
                        }

                        db.Entry(existingEntity).CurrentValues.SetValues(model);
                        db.Entry(existingEntity).State = EntityState.Modified;
                        db.SaveChanges();

                        // 🧹 Clear existing child records before adding new ones
                        db.NO_OF_SCREENS.RemoveRange(db.NO_OF_SCREENS.Where(d => d.ApplId == model.ApplId));

                        // 🆕 Add new child records if available
                        if (model.NO_OF_SCREENS != null && model.NO_OF_SCREENS.Any())
                        {
                            db.NO_OF_SCREENS.AddRange(model.NO_OF_SCREENS);
                        }
                    }

                    // ✅ Call updated methods
                    HandleDocuments(model.ApplId, documents);
                    db.SaveChanges(); // Save the NO_OF_SCREENS changes

                    transaction.Commit();
                    db2Connection.Close();

                    return RedirectToAction("List_of_Application", "Common");
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    db2Connection.Close();
                    Debug.WriteLine($"DB2 Error: {ex.InnerException?.InnerException?.Message}");
                    TempData["ErrorMessage"] = "Database update error. Check logs for details.";
                    return View(model);
                }
                finally
                {
                    if (db2Connection.State == ConnectionState.Open)
                    {
                        db2Connection.Close();
                    }
                }
            }
        }

        private void HandleDocuments(int applId, HttpPostedFileBase[] documents)
        {
            if (documents == null || documents.Length == 0)
            {
                Console.WriteLine("No documents uploaded.");
                return;
            }

            // ✅ Step 1: Verify that applId exists in TRN_REGISTRATION
            bool applIdExists = db.TRN_REGISTRATION.Any(r => r.ApplId == applId);
            if (!applIdExists)
            {
                Console.WriteLine($"Error: ApplId {applId} does not exist in TRN_REGISTRATION.");
                return;
            }

            string uploadPath = Server.MapPath("~/UploadedFiles");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var docList = db.MST_DOCS.Where(d => d.DocActive == 1).ToList();
            if (docList.Count == 0)
            {
                Console.WriteLine("Document list is empty in the database.");
                return;
            }

            var existingDocs = db.TRN_THEATRE_DOCS.Where(d => d.ApplId == applId).ToList();

            for (int i = 0; i < documents.Length; i++)
            {
                var uploadedFile = documents[i];
                if (uploadedFile == null || uploadedFile.ContentLength <= 0)
                {
                    Console.WriteLine($"Skipping file at index {i} - Null or empty.");
                    continue;
                }

                if (i >= docList.Count)
                {
                    Console.WriteLine($"Skipping file at index {i} - No matching document in MST_DOCS.");
                    continue;
                }

                var doc = docList[i];

                string fileExtension = Path.GetExtension(uploadedFile.FileName);
                string formattedFileName = $"TT_{doc.DocName}_{applId}{fileExtension}";
                formattedFileName = SanitizeFileName(formattedFileName);
                string path = Path.Combine(uploadPath, formattedFileName);
                uploadedFile.SaveAs(path);

                var existingDoc = existingDocs.FirstOrDefault(d => d.DocId == doc.DocId);
                if (existingDoc != null)
                {
                    db.TRN_THEATRE_DOCS.Remove(existingDoc);
                    db.SaveChanges();
                }

                db.TRN_THEATRE_DOCS.Add(new TRN_THEATRE_DOCS
                {
                    ApplId = applId,
                    DocId = doc.DocId,
                    DocFilePath = path,
                    CreateUser = "System",
                    CreateDate = DateTime.Now,
                    Active = 1
                });

                db.SaveChanges();
            }
        }


        private void HandleScreens(int applId, string[] seatCapacity = null, string[] screenType = null)
        {
            var existingScreens = db.NO_OF_SCREENS.Where(s => s.ApplId == applId).ToList();

            if (existingScreens.Any())
            {
                db.NO_OF_SCREENS.RemoveRange(existingScreens);
                db.SaveChanges();
            }

            if (seatCapacity != null && screenType != null && seatCapacity.Length == screenType.Length)
            {
                for (int i = 0; i < seatCapacity.Length; i++)
                {
                    if (!string.IsNullOrEmpty(seatCapacity[i]) && !string.IsNullOrEmpty(screenType[i]))
                    {
                        db.NO_OF_SCREENS.Add(new NO_OF_SCREENS
                        {
                            ApplId = applId,
                            AudienceCapacity = int.Parse(seatCapacity[i]),
                            ScreenType = screenType[i]
                        });
                    }
                }
            }

            db.SaveChanges();
        }



        private void UpdateTheaterStatus(TRN_REGISTRATION existingRegistration, string actionType, string rejectReason)
        {
            var dbEntity = db.TRN_REGISTRATION.SingleOrDefault(r => r.ApplId == existingRegistration.ApplId);
            if (dbEntity == null) return;

            db.Entry(dbEntity).Reload();

            if (actionType == "Edit")
            {
                dbEntity.TStatus = "Pending";
            }
            else if (actionType == "Approve")
            {
                dbEntity.TActive = 1;
                dbEntity.TStatus = "Approved";
                dbEntity.RejectReason = null;
            }
            else if (actionType == "Reject")
            {
                dbEntity.TActive = 1;
                dbEntity.TStatus = "Rejected";
                dbEntity.RejectReason = rejectReason;
            }

            dbEntity.UpdateUser = "System";
            dbEntity.UpdateDate = DateTime.Now;
            db.Entry(dbEntity).State = EntityState.Modified;
            db.SaveChanges();
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }




        // ✅ Fetch Active Documents
        private List<MST_DOCS> GetActiveDocuments()
        {
            return db.MST_DOCS
                .Where(d => d.DocActive == 1)
                .Select(d => new  // ✅ FIRST GET ANONYMOUS TYPE
                {
                    DocId = d.DocId,
                    DocName = d.DocName,
                    FILE_SIZE_MB = d.FILE_SIZE_MB,
                    FILE_TYPE = d.FILE_TYPE
                })
                .AsEnumerable() // ✅ SWITCH TO IN-MEMORY PROCESSING
                .Select(d => new MST_DOCS  // ✅ NOW CONVERT TO ENTITY
                {
                    DocId = d.DocId,
                    DocName = d.DocName,
                    FILE_SIZE_MB = d.FILE_SIZE_MB,
                    FILE_TYPE =d.FILE_TYPE
                })
                .ToList();
        }


      
        
    }
}
