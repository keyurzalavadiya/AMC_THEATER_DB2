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
using System.IO; 

namespace AMC_THEATER_1.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        

        public ActionResult Edit(int id, string mode = "edit")
        {
            try
            {
                var model = db.TRN_REGISTRATION
                              .Where(r => r.ApplId == id) 
                              .Include(r => r.NO_OF_SCREENS)
                              .Include(r => r.TRN_THEATRE_DOCS)

                              .FirstOrDefault();

                if (model == null)
                {
                    TempData["ErrorMessage"] = "No record found!";
                    return RedirectToAction("List_of_Application", "Home");
                }
                // ✅ Extract month & year safely from nullable DateTime
                ViewBag.OfflineTaxPaidMonth = model.OfflineTaxPaidMonth?.Month; // Get only month (1-12)
                ViewBag.OfflineTaxPaidYear = model.OfflineTaxPaidYear?.Year;   // Get only year


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


        public ActionResult Registration(int? id, bool isViewPage = false, string mode = "create")
        {
            ViewBag.PageTitle = "Theater Registration";
            ViewBag.Mode = mode;
            ViewBag.Documents = GetActiveDocuments();

            var model = new TRN_REGISTRATION();

            if (id.HasValue)
            {
               
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
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration(TRN_REGISTRATION model, HttpPostedFileBase[] documents, string actionType, string rejectReason = null)
        {
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
               

                        if (model.ApplId == 0) // New Registration
                        {
                            model.CreateDate = DateTime.Now;
                            model.CreateUser = "System";
                            if (!string.IsNullOrEmpty(Request.Form["OfflineTaxPaidYear"]) && int.TryParse(Request.Form["OfflineTaxPaidYear"], out int year))
                            {
                                model.OfflineTaxPaidYear = new DateTime(year, 1, 1);
                                Debug.WriteLine($"Parsed OfflineTaxPaidYear: {model.OfflineTaxPaidYear}");
                            }
                            // ✅ Handling OfflineTaxPaidMonth
                            if (!string.IsNullOrEmpty(Request.Form["OfflineTaxPaidMonth"]) && int.TryParse(Request.Form["OfflineTaxPaidMonth"], out int month) && month >= 1 && month <= 12)
                            {
                                model.OfflineTaxPaidMonth = new DateTime(model.OfflineTaxPaidYear?.Year ?? DateTime.Now.Year, month, 1); // Store as DateTime
                                Debug.WriteLine($"Parsed OfflineTaxPaidMonth: {model.OfflineTaxPaidMonth}");
                            }
                    // ✅ Ensure TTenamentNo is stored correctly
                    if (!string.IsNullOrEmpty(model.TTenamentNo))
                    {
                        model.TTenamentNo = string.Join(",", model.TTenamentNo.Split(',').Select(t => t.Trim()));
                    }

                    db.TRN_REGISTRATION.Add(model);
                    db.SaveChanges();
                    db.Database.ExecuteSqlCommand("CALL AMCTHEATER.ADD_THEATER(@p0)", model.ApplId);
                }
                else
                {
                    var existingEntity = db.TRN_REGISTRATION.Find(model.ApplId);
                    UpdateTheaterStatus(existingEntity, actionType, rejectReason);

                    db.Entry(existingEntity).Reload(); // ✅ Ensure we have the latest TId

                    // ✅ First, delete old NO_OF_SCREENS records
                    db.NO_OF_SCREENS.RemoveRange(db.NO_OF_SCREENS.Where(d => d.ApplId == model.ApplId));
                    db.SaveChanges(); // ✅ Ensure deletion is committed

                    // ✅ Then, add new NO_OF_SCREENS records if available
                    if (model.NO_OF_SCREENS?.Any() == true)
                    {
                        foreach (var screen in model.NO_OF_SCREENS)
                        {
                            screen.TId = existingEntity.TId; // ✅ Assign the correct TId before inserting
                            screen.ApplId = model.ApplId;
                        }

                        db.NO_OF_SCREENS.AddRange(model.NO_OF_SCREENS);
                        db.SaveChanges(); // ✅ Save new records
                    }
                }


                // ✅ Ensure documents are handled **after** ApplId is saved
                if (documents != null && documents.Length > 0)
                        {
                            HandleDocuments(model.ApplId, documents);
                        }

                        db.SaveChanges(); // Final save

                        transaction.Commit();
                        return RedirectToAction("List_of_Application", "Home");
                    }
                }


        private void UpdateTheaterStatus(TRN_REGISTRATION existingRegistration, string actionType, string rejectReason)
        {
            var dbEntity = db.TRN_REGISTRATION.SingleOrDefault(r => r.ApplId == existingRegistration.ApplId);

            if (dbEntity == null)
            {
                throw new Exception($"❌ Error: Record with APPLICATION_ID = {existingRegistration.ApplId} not found. It may have been deleted.");
            }

            db.Entry(dbEntity).Reload(); // ✅ Reload to avoid concurrency issues

            if (actionType == "Edit")
            {
                dbEntity.TStatus = "Pending";
            }
            else if (actionType == "Approve")
            {
                dbEntity.TActive = 1;
                dbEntity.TStatus = "Approved";
                dbEntity.RejectReason = null;

                if (string.IsNullOrEmpty(dbEntity.TId))
                {
                    dbEntity.TId = GenerateNextTId(); // ✅ Generate new TId if it's NULL
                }

                Console.WriteLine($"🔥 Generated TId: {dbEntity.TId}"); // ✅ Debugging Output

                dbEntity.UpdateUser = "System";
                dbEntity.UpdateDate = DateTime.Now;

                db.Entry(dbEntity).State = EntityState.Modified;
                db.SaveChanges(); // ✅ Save TRN_REGISTRATION changes first

                // ✅ Fetch NO_OF_SCREENS records AFTER saving TId
                var screenEntities = db.NO_OF_SCREENS.Where(s => s.ApplId == dbEntity.ApplId).ToList();

                if (!screenEntities.Any())
                {
                    throw new Exception($"❌ No records found in NO_OF_SCREENS for ApplId: {dbEntity.ApplId}");
                }

                // ✅ Now update `TId` in `NO_OF_SCREENS`
                foreach (var screen in screenEntities)
                {
                    Console.WriteLine($"Before Update -> ScreenId: {screen.ScreenId}, Old TId: {screen.TId}, New TId: {dbEntity.TId}");

                    screen.TId = dbEntity.TId; // ✅ Assign the correct TId
                    db.Entry(screen).Property(x => x.TId).IsModified = true; // ✅ Force EF to detect change
                }

                int affectedRows = db.SaveChanges();
                Console.WriteLine($"🔥 NO_OF_SCREENS updated rows: {affectedRows}");

                // ✅ If EF6 fails, force the update using SQL
                if (affectedRows == 0)
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE NO_OF_SCREENS SET TId = @p0 WHERE ApplId = @p1",
                        dbEntity.TId, dbEntity.ApplId
                    );
                    Console.WriteLine("🔥 Forced SQL Update on NO_OF_SCREENS");
                }
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

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Concurrency issue detected for APPLICATION_ID: {existingRegistration.ApplId}. Retrying update...");

                db.Entry(dbEntity).Reload();
                db.SaveChanges();
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


        private string GenerateNextTId()
        {
            var lastTId = db.TRN_REGISTRATION
                .Where(r => r.TId.StartsWith("T"))
                .OrderByDescending(r => r.TId)
                .Select(r => r.TId)
                .FirstOrDefault();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastTId) && lastTId.Length > 1)
            {
                string numberPart = lastTId.Substring(1); // Extract number part (e.g., "0001")
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"T{nextNumber:D4}"; // Format as T0001, T0002, etc.
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

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
